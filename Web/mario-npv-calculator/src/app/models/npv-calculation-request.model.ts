export interface NPVCalculationRequest {
    cashFlows: number[];
    lowerBound: number;
    upperBound: number;
    increment: number;
    currency?: string;
}