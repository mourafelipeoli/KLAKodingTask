import type { FeatureAvailability } from "./appInfo";

export interface HostingFacts {
  onAws: boolean;
  region: string | null;
  regionName: string | null;
  availabilityZone: string | null;
  compute: string | null;
  vcpu: number | null;
  memoryMiB: number | null;
  startedAt: string | null;
  runtime: string;
  operatingSystem: string;
  architecture: string;
}

export interface ServiceFacts {
  running: number;
  desired: number;
  healthyTargets: number | null;
  registeredTargets: number | null;
  rolloutState: string | null;
  lastDeployedAt: string | null;
  automaticRollback: boolean;
  publicIpAddress: boolean;
}

export interface TrafficFacts {
  requests24h: number | null;
  requestsPerHour: number[] | null;
  averageResponseMs24h: number | null;
  cpuPercent1h: number | null;
  memoryPercent1h: number | null;
}

export interface BuildFacts {
  version: string | null;
  commit: string | null;
  commitUrl: string | null;
  builtAt: string | null;
}

export interface DataFacts {
  engine: string;
  version: string | null;
  managedBy: string | null;
  persistent: boolean;
  multiAz: boolean | null;
}

export interface SecurityFacts {
  https: boolean;
  certificateValidUntil: string | null;
  limitsOutsideCode: boolean;
}

export interface GuardrailFacts {
  killSwitch: boolean;
  aiCallsToday: number;
  aiCallsPerDay: number;
  aiSpendThisMonthUsd: number;
  aiMonthlyBudgetUsd: number;
  emailsToday: number;
  emailsPerDay: number;
  whatsAppToday: number;
  whatsAppPerDay: number;
}

export interface InfrastructureReport {
  checkedAt: string;
  hosting: HostingFacts;
  service: ServiceFacts | null;
  traffic: TrafficFacts | null;
  build: BuildFacts;
  data: DataFacts;
  security: SecurityFacts;
  guardrails: GuardrailFacts;
  features: FeatureAvailability;
}
