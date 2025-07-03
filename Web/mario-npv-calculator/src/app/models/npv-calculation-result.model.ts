import { NPVCalculationMetadata } from "./npv-calculation-metadata.model";
import { NPVResultItem } from "./npv-result-item.model";

export interface NPVCalculationResult {
    results: NPVResultItem[];
    metadata: NPVCalculationMetadata;
}