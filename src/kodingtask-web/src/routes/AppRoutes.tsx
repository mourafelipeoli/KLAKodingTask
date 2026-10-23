import { Route, Routes } from "react-router-dom";
import ConvertPage from "../pages/ConvertPage";
import NotFoundPage from "../pages/NotFoundPage";
import { ROUTES } from "./routePaths";

export default function AppRoutes() {
  return (
    <Routes>
      <Route path={ROUTES.home} element={<ConvertPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
