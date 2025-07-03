import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormArray, FormControl } from '@angular/forms';
@Component({
  selector: 'app-cash-flow-input',
  templateUrl: './cash-flow-input.component.html',
  styleUrls: ['./cash-flow-input.component.scss']
})
export class CashFlowInputComponent {
  @Input() cashFlows!: FormArray;
  @Output() addCashFlow = new EventEmitter<void>();
  @Output() removeCashFlow = new EventEmitter<number>();
  @Output() updateCashFlow = new EventEmitter<{ index: number; value: string }>();

  // Helper method to get FormControl at specific index
  getControlAt(index: number): FormControl {
    return this.cashFlows.at(index) as FormControl;
  }

  // Helper getter to return controls as FormControl array
  get formControls(): FormControl[] {
    return this.cashFlows.controls as FormControl[];
  }

  onAddCashFlow(): void {
    this.addCashFlow.emit();
  }

  onRemoveCashFlow(index: number): void {
    this.removeCashFlow.emit(index);
  }

  onUpdateCashFlow(index: number, event: Event): void {
    const target = event.target as HTMLInputElement;
    this.updateCashFlow.emit({ index, value: target.value });
  }

  trackByIndex(index: number): number {
    return index;
  }
}
