import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { NPVCalculationRequest } from 'src/app/models/npv-calculation-request.model';
import { NPVCalculationResult } from 'src/app/models/npv-calculation-result.model';
import { NPVCalculationService } from '../../services/npv-calculation.service';

@Component({
  selector: 'app-npv-calculator',
  templateUrl: './npv-calculator.component.html',
  styleUrls: ['./npv-calculator.component.scss']
})
export class NPVCalculatorComponent implements OnInit, OnDestroy {
  calculatorForm: FormGroup;
  results: NPVCalculationResult | null = null;
  loading = false;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private npvService: NPVCalculationService
  ) {
    this.calculatorForm = this.createForm();
  }

  ngOnInit(): void {
    this.initializeDefaultValues();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      cashFlows: this.fb.array([]),
      lowerBound: [1, [Validators.required, Validators.min(0), Validators.max(100)]],
      upperBound: [15, [Validators.required, Validators.min(0), Validators.max(100)]],
      increment: [0.25, [Validators.required, Validators.min(0.01), Validators.max(100)]],
      currency: ['USD', Validators.required]
    });
  }

  private initializeDefaultValues(): void {
    const defaultCashFlows = [-1000, 300, 300, 300, 300, 300];
    defaultCashFlows.forEach(cf => this.addCashFlow(cf));
  }

  get cashFlows(): FormArray {
    return this.calculatorForm.get('cashFlows') as FormArray;
  }

  addCashFlow(value: number = 0): void {
    const cashFlowControl = this.fb.control(value, [Validators.required, Validators.pattern(/^-?\d+\.?\d*$/)]);
    this.cashFlows.push(cashFlowControl);
  }

  removeCashFlow(index: number): void {
    if (this.cashFlows.length > 1) {
      this.cashFlows.removeAt(index);
    }
  }

  updateCashFlow(index: number, value: string): void {
    const numValue = parseFloat(value);
    if (!isNaN(numValue)) {
      this.cashFlows.at(index).setValue(numValue);
    }
  }

  onCalculate(): void {
    if (this.calculatorForm.invalid) {
      this.markFormGroupTouched(this.calculatorForm);
      return;
    }

    this.loading = true;
    this.error = null;
    this.results = null;

    const request: NPVCalculationRequest = {
      cashFlows: this.cashFlows.value,
      lowerBound: this.calculatorForm.value.lowerBound,
      upperBound: this.calculatorForm.value.upperBound,
      increment: this.calculatorForm.value.increment,
      currency: this.calculatorForm.value.currency
    };

    const validationErrors = this.npvService.validateRequest(request);
    if (validationErrors.length > 0) {
      this.error = validationErrors.join('. ');
      this.loading = false;
      return;
    }

    this.npvService.calculateNPVRange(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.results = result;
          this.loading = false;
        },
        error: (error) => {
          this.error = error.message || 'An error occurred while calculating NPV';
          this.loading = false;
        }
      });
  }

  onReset(): void {
    this.calculatorForm.reset({
      lowerBound: 1,
      upperBound: 15,
      increment: 0.25,
      currency: 'USD'
    });
    this.cashFlows.clear();
    this.initializeDefaultValues();
    this.results = null;
    this.error = null;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control instanceof FormArray) {
        control.controls.forEach(c => c.markAsTouched());
      }
    });
  }
}
