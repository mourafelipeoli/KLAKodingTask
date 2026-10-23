import { useState } from "react";
import { useText } from "../../i18n/useText";
import { formatHour, formatNumber } from "../../utils/formatFacts";

interface RequestSparklineProps {
  /** Requests per hour, oldest first; the last value is the current, unfinished hour. */
  perHour: readonly number[];
  /** When the figures were read; the last bar is the hour containing this instant. */
  checkedAt: string;
}

const HOUR_MS = 60 * 60 * 1000;

interface Hour {
  start: Date;
  count: number;
}

function toHours(perHour: readonly number[], checkedAt: string): Hour[] {
  const current = new Date(checkedAt);
  current.setUTCMinutes(0, 0, 0);
  return perHour.map((count, index) => ({
    start: new Date(current.getTime() - (perHour.length - 1 - index) * HOUR_MS),
    count,
  }));
}

// One series, so no legend: the caption names it. Bars sit on the baseline with 2px gaps;
// the current hour wears the accent, the others the quieter step of the same hue. Hovering
// an hour shows its figure in the readout line (the current hour by default); a hidden
// table carries every value for keyboard and screen-reader users.
export default function RequestSparkline({ perHour, checkedAt }: RequestSparklineProps) {
  const { t, locale } = useText();
  const hours = toHours(perHour, checkedAt);
  const lastIndex = hours.length - 1;
  const [activeIndex, setActiveIndex] = useState(lastIndex);
  const highest = Math.max(1, ...perHour);
  const active = hours[activeIndex] ?? hours[lastIndex];

  return (
    <figure className="sparkline">
      <figcaption className="sparkline__title">
        {t("Requests per hour, last 24 hours")}
      </figcaption>
      <div
        className="sparkline__plot"
        aria-hidden="true"
        onMouseLeave={() => setActiveIndex(lastIndex)}
      >
        {hours.map((hour, index) => (
          <span
            key={hour.start.toISOString()}
            className={`sparkline__slot${index === activeIndex ? " sparkline__slot--active" : ""}`}
            onMouseEnter={() => setActiveIndex(index)}
          >
            <span
              className={`sparkline__bar${index === lastIndex ? " sparkline__bar--current" : ""}${hour.count === 0 ? " sparkline__bar--empty" : ""}`}
              style={{
                height: hour.count === 0 ? undefined : `${(hour.count / highest) * 100}%`,
              }}
            />
          </span>
        ))}
      </div>
      {active && (
        <p className="sparkline__readout">
          {t("Requests from {{time}}: {{count}}", {
            time: formatHour(active.start, locale),
            count: formatNumber(active.count, locale),
          })}
        </p>
      )}
      <table className="visually-hidden">
        <caption>{t("Requests per hour, last 24 hours")}</caption>
        <tbody>
          {hours.map((hour) => (
            <tr key={hour.start.toISOString()}>
              <th scope="row">{formatHour(hour.start, locale)}</th>
              <td>{formatNumber(hour.count, locale)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </figure>
  );
}
