export interface Account {
  isSignedIn: boolean;
  email: string | null;
  emailConfirmed: boolean;
  /** Masked, e.g. "+49*******5678"; the full number never comes back from the server. */
  whatsAppNumber: string | null;
  deliveriesLeftToday: number | null;
  /** Signing out of such a session also ends the one at fmID. */
  signedInWithFmId: boolean;
}

export interface RegisterResult {
  account: Account;
  confirmationSent: boolean;
}

export interface PhoneCodeResult {
  maskedNumber: string;
}

export interface DeliveryResult {
  maskedDestination: string;
  deliveriesLeftToday: number;
}

export const SIGNED_OUT: Account = {
  isSignedIn: false,
  email: null,
  emailConfirmed: false,
  whatsAppNumber: null,
  deliveriesLeftToday: null,
  signedInWithFmId: false,
};
