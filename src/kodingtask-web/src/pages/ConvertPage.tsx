import { useCallback, useEffect, useRef, useState, useTransition } from "react";
import { useSearchParams } from "react-router-dom";
import DeployedVersionDialog from "../components/DeployedVersionDialog";
import AiAnswerPanel from "../components/converter/AiAnswerPanel";
import ConverterForm from "../components/converter/ConverterForm";
import HowItWorks from "../components/converter/HowItWorks";
import LanguageComparison from "../components/converter/LanguageComparison";
import ResultCard from "../components/converter/ResultCard";
import SendResultPanel from "../components/converter/SendResultPanel";
import StatsStrip from "../components/converter/StatsStrip";
import { LOCALES } from "../config/locales";
import { useDeployedVersion } from "../hooks/useDeployedVersion";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { conversionService } from "../services/conversionService";
import { ApiError, NETWORK_ERROR, isAbortError } from "../services/httpClient";
import type {
  AiAnswer,
  ConversionResult,
  DeliveryChannel,
  OutputLanguage,
} from "../types/conversion";
import { Engine } from "../types/conversion";
import { AI_UNAVAILABLE } from "../utils/aiErrors";
import { conversionErrorKey } from "../utils/conversionErrors";
import { ConverterSection, converterPath, readLinkTarget } from "../utils/converterLink";
import "./ConvertPage.css";

// Used until the server's list arrives (and if it never does): the same codes as the UI.
const FALLBACK_LANGUAGES: readonly OutputLanguage[] = LOCALES.map((locale) => ({
  code: locale.code,
  englishName: locale.nativeName,
  nativeName: locale.nativeName,
}));

function errorKeyOf(error: unknown): string {
  return conversionErrorKey(error instanceof ApiError ? error.code : NETWORK_ERROR);
}

function aiErrorOf(error: unknown): string {
  return error instanceof ApiError && error.code === "TooManyRequests"
    ? "AiBusy"
    : "AiFailed";
}

export default function ConvertPage() {
  const { t, locale } = useText();
  const { isDeployedVersion, urlFor } = useDeployedVersion();
  useDocumentTitle(t("Dollars in words"));

  const [searchParams] = useSearchParams();
  const [amount, setAmount] = useState(() => searchParams.get("amount") ?? "");
  // Null means "follow the interface language"; a choice in the form overrides it.
  const [chosenLanguage, setChosenLanguage] = useState(() =>
    searchParams.get("language"),
  );
  const outputLanguage = chosenLanguage ?? locale;
  const [engine, setEngine] = useState<Engine>(() =>
    searchParams.get("engine") === Engine.Ai ? Engine.Ai : Engine.Rules,
  );

  const [languages, setLanguages] =
    useState<readonly OutputLanguage[]>(FALLBACK_LANGUAGES);
  const [result, setResult] = useState<ConversionResult | null>(null);
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [aiAnswer, setAiAnswer] = useState<AiAnswer | null>(null);
  const [aiError, setAiError] = useState<string | null>(null);
  const [comparison, setComparison] = useState<readonly ConversionResult[] | null>(null);
  const [isComparisonOpen, setIsComparisonOpen] = useState(false);
  // A copy without credentials offers the deployed version instead of an AI answer or a
  // delivery it cannot produce; closing the offer keeps the visitor here.
  const [isAiOfferDismissed, setIsAiOfferDismissed] = useState(false);
  const [deliveryOffer, setDeliveryOffer] = useState<DeliveryChannel | null>(null);
  const [isConverting, startConversion] = useTransition();
  const [isAsking, startAsking] = useTransition();
  const [isComparing, startComparison] = useTransition();

  // The amount the visitor last asked for. Re-converted when the output language changes.
  const requestedAmount = useRef<string | null>(searchParams.get("amount"));
  const engineChoice = useRef(engine);
  const comparisonWanted = useRef(false);
  const conversionAbort = useRef<AbortController | null>(null);
  const aiAbort = useRef<AbortController | null>(null);
  const comparisonAbort = useRef<AbortController | null>(null);
  // Where a link (a hand-over from a local copy, typically) asked to land: scrolled to once,
  // as soon as the amount it carries has been converted.
  const linkTarget = useRef(readLinkTarget(searchParams, window.location.hash));

  useEffect(() => {
    const target = linkTarget.current;
    if (!result || !target.section) {
      return;
    }
    linkTarget.current = { section: null, channel: null };
    document.getElementById(target.section)?.scrollIntoView({ block: "start" });
    if (target.channel) {
      document
        .querySelector<HTMLButtonElement>(`[data-channel="${target.channel}"]`)
        ?.focus({ preventScroll: true });
    }
  }, [result]);

  useEffect(() => {
    const controller = new AbortController();
    conversionService
      .languages(controller.signal)
      .then(setLanguages)
      .catch(() => {
        // Keep the built-in list; conversion errors are reported where they happen.
      });
    return () => controller.abort();
  }, []);

  const compare = useCallback((value: string) => {
    comparisonAbort.current?.abort();
    const controller = new AbortController();
    comparisonAbort.current = controller;
    startComparison(async () => {
      try {
        setComparison(
          await conversionService.convertToAllLanguages(value, controller.signal),
        );
      } catch (error) {
        if (!isAbortError(error)) {
          setComparison(null);
          setErrorKey(errorKeyOf(error));
        }
      }
    });
  }, []);

  // Always asks the server, even before /api/app-info has answered: an instance without
  // AI answers "AiUnavailable", so there is no race between the two first requests.
  const askAi = useCallback((value: string, language: string) => {
    aiAbort.current?.abort();
    const controller = new AbortController();
    aiAbort.current = controller;
    setIsAiOfferDismissed(false);
    startAsking(async () => {
      try {
        const answer = await conversionService.convertWithAi(
          value,
          language,
          controller.signal,
        );
        setAiAnswer(answer.ai);
        setAiError(answer.aiError);
      } catch (error) {
        if (!isAbortError(error)) {
          setAiAnswer(null);
          setAiError(aiErrorOf(error));
        }
      }
    });
  }, []);

  const convert = useCallback(
    (value: string, language: string) => {
      requestedAmount.current = value;
      conversionAbort.current?.abort();
      aiAbort.current?.abort();
      const controller = new AbortController();
      conversionAbort.current = controller;
      startConversion(async () => {
        try {
          const converted = await conversionService.convert(
            value,
            language,
            controller.signal,
          );
          setResult(converted);
          setErrorKey(null);
          setAiAnswer(null);
          setAiError(null);
          if (engineChoice.current === Engine.Ai) {
            askAi(value, language);
          }
          if (comparisonWanted.current) {
            compare(value);
          }
        } catch (error) {
          if (!isAbortError(error)) {
            setResult(null);
            setErrorKey(errorKeyOf(error));
          }
        }
      });
    },
    [askAi, compare],
  );

  // Converts the amount from a shared link on arrival, and converts again whenever the
  // output language changes (a choice in the form, or the interface language it follows).
  useEffect(() => {
    if (requestedAmount.current !== null) {
      convert(requestedAmount.current, outputLanguage);
    }
  }, [outputLanguage, convert]);

  function handleEngineChange(next: Engine) {
    engineChoice.current = next;
    setEngine(next);
    setAiAnswer(null);
    setAiError(null);
    if (next === Engine.Ai && result) {
      askAi(result.amount, result.language);
    }
  }

  function handleToggleComparison() {
    const next = !isComparisonOpen;
    comparisonWanted.current = next;
    setIsComparisonOpen(next);
    if (next && result) {
      compare(result.amount);
    }
  }

  function handleExample(example: string) {
    setAmount(example);
    convert(example, outputLanguage);
  }

  const nameOf = (code: string) =>
    languages.find((language) => language.code === code)?.nativeName ?? code;

  const isAiOfferOpen =
    engine === Engine.Ai &&
    aiError === AI_UNAVAILABLE &&
    !isDeployedVersion &&
    !isAiOfferDismissed;
  const deployedUrl = urlFor(
    converterPath({
      amount: result?.amount ?? amount,
      language: result?.language ?? outputLanguage,
      engine,
      uiLocale: locale,
      section: deliveryOffer ? ConverterSection.Send : ConverterSection.Result,
      channel: deliveryOffer ?? undefined,
    }),
  );

  return (
    <>
      <section className="hero">
        <div className="container hero__inner">
          <p className="eyebrow">{t("Currency to words")}</p>
          <h1 className="hero__title">{t("Write any dollar amount in words")}</h1>
          <p className="hero__lead">
            {t(
              "Type an amount up to {{maximum}} and read it in {{count}} languages. The conversion runs on the server.",
              { maximum: "999 999 999,99", count: languages.length },
            )}
          </p>
        </div>
      </section>

      <div className="container convert-page__body">
        <div className="card card--accent convert-page__converter">
          <ConverterForm
            amount={amount}
            language={outputLanguage}
            languages={languages}
            engine={engine}
            isConverting={isConverting}
            errorKey={errorKey}
            onAmountChange={setAmount}
            onLanguageChange={setChosenLanguage}
            onEngineChange={handleEngineChange}
            onSubmit={() => convert(amount, outputLanguage)}
            onExample={handleExample}
          />
          {result && (
            <ResultCard
              result={result}
              engine={engine}
              languageName={nameOf(result.language)}
              isComparisonOpen={isComparisonOpen}
              onToggleComparison={handleToggleComparison}
            >
              {engine === Engine.Ai && (
                <AiAnswerPanel
                  answer={aiAnswer}
                  errorCode={aiError}
                  language={result.language}
                  isLoading={isAsking}
                />
              )}
            </ResultCard>
          )}
        </div>

        {isComparisonOpen && result && (
          <LanguageComparison
            results={comparison}
            isLoading={isComparing}
            nameOf={nameOf}
          />
        )}

        {result && <SendResultPanel onUnavailable={setDeliveryOffer} />}
      </div>

      <StatsStrip
        languageCount={languages.length}
        frenchVariantCount={
          languages.filter((language) => language.code.startsWith("fr-")).length
        }
      />
      <HowItWorks />

      <DeployedVersionDialog
        isOpen={isAiOfferOpen || deliveryOffer !== null}
        url={deployedUrl}
        onClose={() => {
          setIsAiOfferDismissed(true);
          setDeliveryOffer(null);
        }}
      />
    </>
  );
}
