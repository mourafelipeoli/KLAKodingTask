export interface Account {
  isSignedIn: boolean;
  email: string | null;
  /** Masked, e.g. "+49*******5678"; the full number never comes back from the server. */
  whatsAppNumber: string | null;
  deliveriesLeftToday: number | null;
}

export interface DeliveryResult {
  maskedDestination: string;
  deliveriesLeftToday: number;
}

export const SIGNED_OUT: Account = {
  isSignedIn: false,
  email: null,
  whatsAppNumber: null,
  deliveriesLeftToday: null,
};
