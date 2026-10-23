# PublicSiteOptions

Links the application shows about itself, bound from `PublicSite`.

- `Url`: the hosted instance where every feature is configured. A local instance that lacks
  a feature points the user there.
- `RepositoryUrl`: the public source code. Empty hides the link in the footer.
- `Url`: also decides, in the browser, whether this page *is* the hosted instance (same
  origin) or a local copy that should offer to continue there.
- `AuthorName` and `AuthorUrl`: who built it and where to find their portfolio, shown in the
  purple top bar (as the author's own wordmark) and in the footer. Both are needed for the
  links to appear.
- `AuthorTerminalUrl`: the author's interactive terminal page, the second link of the top
  bar. Empty hides it.

A team that deploys its own copy changes these values, not the code.
