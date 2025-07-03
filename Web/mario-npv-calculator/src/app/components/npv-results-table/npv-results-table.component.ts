import { Component, Input } from '@angular/core';
import { NPVResultItem } from 'src/app/models/npv-result-item.model';

@Component({
  selector: 'app-npv-results-table',
  templateUrl: './npv-results-table.component.html',
  styleUrls: ['./npv-results-table.component.scss']
})
export class NPVResultsTableComponent {
  @Input() results: NPVResultItem[] = [];

  getNPVClass(npv: number): string {
    return npv >= 0 ? 'positive' : 'negative';
  }

  formatCurrency(value: number, currency: string): string {
    return `${currency} ${value.toFixed(2)}`;
  }
}
