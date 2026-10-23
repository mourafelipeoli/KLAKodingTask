// Applies the colour theme before the first paint. Light is the standard, as on kla.com;
// dark only when the visitor chose it with the theme button (remembered in this browser).
// It is an external file, not an inline script, so the content security policy can forbid
// inline scripts entirely.
(function () {
  var theme = "light";
  try {
    if (window.localStorage.getItem("kodingtask.theme") === "dark") {
      theme = "dark";
    }
  } catch (error) {
    theme = "light";
  }
  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;
})();
