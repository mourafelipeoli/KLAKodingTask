import { Route, Routes } from "react-router-dom";
import ConfirmEmailPage from "../pages/ConfirmEmailPage";
import ConvertPage from "../pages/ConvertPage";
import ForgotPasswordPage from "../pages/ForgotPasswordPage";
import InfrastructurePage from "../pages/InfrastructurePage";
import NotFoundPage from "../pages/NotFoundPage";
import RegisterPage from "../pages/RegisterPage";
import ResetPasswordPage from "../pages/ResetPasswordPage";
import SignInPage from "../pages/SignInPage";
import { ROUTES } from "./routePaths";

export default function AppRoutes() {
  return (
    <Routes>
      <Route path={ROUTES.home} element={<ConvertPage />} />
      <Route path={ROUTES.infrastructure} element={<InfrastructurePage />} />
      <Route path={ROUTES.signIn} element={<SignInPage />} />
      <Route path={ROUTES.register} element={<RegisterPage />} />
      <Route path={ROUTES.confirmEmail} element={<ConfirmEmailPage />} />
      <Route path={ROUTES.forgotPassword} element={<ForgotPasswordPage />} />
      <Route path={ROUTES.resetPassword} element={<ResetPasswordPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
