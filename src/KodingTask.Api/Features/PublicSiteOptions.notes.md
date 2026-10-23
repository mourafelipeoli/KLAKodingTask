# PublicSiteOptions

Links the application shows about itself, bound from `PublicSite`.

- `Url`: the hosted instance where every feature is configured. A local instance that lacks
  a feature points the user there.
- `RepositoryUrl`: the public source code. Empty hides the link in the footer.
- `AuthorName` and `AuthorUrl`: who built it and where to find their portfolio, shown in the
  header and the footer. Both are needed for the link to appear.

A team that deploys its own copy changes these values, not the code.
