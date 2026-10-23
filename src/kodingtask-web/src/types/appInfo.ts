export interface FeatureAvailability {
  email: boolean;
  whatsApp: boolean;
  ai: boolean;
  machineTranslation: boolean;
  externalLogin: boolean;
  deliveries: boolean;
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
}
