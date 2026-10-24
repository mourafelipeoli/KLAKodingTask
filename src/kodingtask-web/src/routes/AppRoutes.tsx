import { Route, Routes } from "react-router-dom";
import ConvertPage from "../pages/ConvertPage";
import InfrastructurePage from "../pages/InfrastructurePage";
import NotFoundPage from "../pages/NotFoundPage";
import SignInPage from "../pages/SignInPage";
import { ROUTES } from "./routePaths";

export default function AppRoutes() {
  return (
    <Routes>
      <Route path={ROUTES.home} element={<ConvertPage />} />
      <Route path={ROUTES.infrastructure} element={<InfrastructurePage />} />
      <Route path={ROUTES.signIn} element={<SignInPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
