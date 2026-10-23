// Applies the saved (or system) colour theme before the first paint, so a dark-theme
// visitor never sees a white flash. It is an external file, not an inline script, so the
// content security policy can forbid inline scripts entirely.
(function () {
  var theme = "light";
  try {
    var stored = window.localStorage.getItem("kodingtask.theme");
    if (stored === "light" || stored === "dark") {
      theme = stored;
    } else if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
      theme = "dark";
    }
  } catch (error) {
    theme = "light";
  }
  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;
})();
