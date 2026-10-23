import { BrowserRouter } from "react-router-dom";
import { AppInfoProvider } from "./app/AppInfoProvider";
import Layout from "./components/Layout";
import { I18nProvider } from "./i18n/I18nProvider";
import AppRoutes from "./routes/AppRoutes";

export default function App() {
  return (
    <BrowserRouter>
      <I18nProvider>
        <AppInfoProvider>
          <Layout>
            <AppRoutes />
          </Layout>
        </AppInfoProvider>
      </I18nProvider>
    </BrowserRouter>
  );
}
