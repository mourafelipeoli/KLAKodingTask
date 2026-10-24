export interface FeatureAvailability {
  email: boolean;
  whatsApp: boolean;
  ai: boolean;
  machineTranslation: boolean;
  externalLogin: boolean;
  deliveries: boolean;
  accounts: boolean;
}

export interface Author {
  name: string;
  url: string;
  terminalUrl: string | null;
}

export interface AppInfo {
  version: string | null;
  commit: string | null;
  publicSiteUrl: string;
  repositoryUrl: string | null;
  author: Author | null;
  features: FeatureAvailability;
  /** fmID's account page, where people add and confirm their WhatsApp number. */
  fmIdAccountUrl: string | null;
}
