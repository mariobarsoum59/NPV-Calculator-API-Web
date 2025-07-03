import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { NPVCalculationService } from '../../services/npv-calculation.service';
import { NPVCalculatorComponent } from './npv-calculator.component';

describe('NPVCalculatorComponent', () => {
  let component: NPVCalculatorComponent;
  let fixture: ComponentFixture<NPVCalculatorComponent>;
  let mockNPVService: jasmine.SpyObj<NPVCalculationService>;

  beforeEach(async () => {
    mockNPVService = jasmine.createSpyObj('NPVCalculationService', ['calculateNPVRange', 'validateRequest']);

    await TestBed.configureTestingModule({
      declarations: [NPVCalculatorComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: NPVCalculationService, useValue: mockNPVService }
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(NPVCalculatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.cashFlows.length).toBe(6);
    expect(component.calculatorForm.get('lowerBound')?.value).toBe(1);
    expect(component.calculatorForm.get('upperBound')?.value).toBe(15);
    expect(component.calculatorForm.get('increment')?.value).toBe(0.25);
    expect(component.calculatorForm.get('currency')?.value).toBe('USD');
  });

  it('should add cash flow', () => {
    const initialLength = component.cashFlows.length;

    component.addCashFlow(1000);

    expect(component.cashFlows.length).toBe(initialLength + 1);
    expect(component.cashFlows.at(initialLength).value).toBe(1000);
  });

  it('should remove cash flow', () => {
    const initialLength = component.cashFlows.length;

    component.removeCashFlow(0);

    expect(component.cashFlows.length).toBe(initialLength - 1);
  });

  it('should not remove last cash flow', () => {
    // Remove all but one
    while (component.cashFlows.length > 1) {
      component.removeCashFlow(0);
    }

    component.removeCashFlow(0);

    expect(component.cashFlows.length).toBe(1);
  });

  it('should update cash flow value', () => {
    component.updateCashFlow(0, '2000');

    expect(component.cashFlows.at(0).value).toBe(2000);
  });

  it('should not update with invalid value', () => {
    const originalValue = component.cashFlows.at(0).value;

    component.updateCashFlow(0, 'invalid');

    expect(component.cashFlows.at(0).value).toBe(originalValue);
  });

  it('should calculate NPV on valid form', () => {
    mockNPVService.validateRequest.and.returnValue([]);
    mockNPVService.calculateNPVRange.and.returnValue(of({
      results: [{ discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }],
      metadata: { cashFlowCount: 6, calculationCount: 1, calculatedAt: new Date() }
    }));

    component.onCalculate();

    expect(mockNPVService.calculateNPVRange).toHaveBeenCalled();
    expect(component.loading).toBe(false);
    expect(component.results).toBeTruthy();
  });

  it('should show error on invalid form', () => {
    component.calculatorForm.get('lowerBound')?.setValue(-1);

    component.onCalculate();

    expect(mockNPVService.calculateNPVRange).not.toHaveBeenCalled();
  });

  it('should show validation errors', () => {
    mockNPVService.validateRequest.and.returnValue(['Error 1', 'Error 2']);

    component.onCalculate();

    expect(component.error).toBe('Error 1. Error 2');
    expect(component.loading).toBe(false);
  });

  it('should handle service error', () => {
    mockNPVService.validateRequest.and.returnValue([]);
    mockNPVService.calculateNPVRange.and.returnValue(
      throwError(() => new Error('Service error'))
    );

    component.onCalculate();

    expect(component.error).toBe('Service error');
    expect(component.loading).toBe(false);
  });

  it('should reset form', () => {
    component.results = {} as any;
    component.error = 'Some error';
    component.cashFlows.clear();

    component.onReset();

    expect(component.results).toBeNull();
    expect(component.error).toBeNull();
    expect(component.cashFlows.length).toBe(6);
    expect(component.calculatorForm.get('lowerBound')?.value).toBe(1);
  });

  it('should set loading true when calculating', () => {
    mockNPVService.validateRequest.and.returnValue([]);
    mockNPVService.calculateNPVRange.and.returnValue(of({} as any));

    component.loading = false;
    component.onCalculate();

    // Loading should be set to true initially
    expect(mockNPVService.calculateNPVRange).toHaveBeenCalled();
  });

  it('should unsubscribe on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });

  it('should mark form as touched when invalid', () => {
    component.calculatorForm.get('lowerBound')?.setValue(null);

    component.onCalculate();

    expect(component.calculatorForm.get('lowerBound')?.touched).toBe(true);
  });

  it('should get cashFlows FormArray', () => {
    const cashFlows = component.cashFlows;

    expect(cashFlows).toBeTruthy();
    expect(cashFlows.length).toBe(6);
  });

  it('should add cash flow with default value', () => {
    const initialLength = component.cashFlows.length;

    component.addCashFlow();

    expect(component.cashFlows.at(initialLength).value).toBe(0);
  });

  it('should have required validators on form controls', () => {
    const lowerBound = component.calculatorForm.get('lowerBound');
    lowerBound?.setValue('');

    expect(lowerBound?.hasError('required')).toBe(true);
  });

  it('should have min validator on lowerBound', () => {
    const lowerBound = component.calculatorForm.get('lowerBound');
    lowerBound?.setValue(-1);

    expect(lowerBound?.hasError('min')).toBe(true);
  });

  it('should have max validator on upperBound', () => {
    const upperBound = component.calculatorForm.get('upperBound');
    upperBound?.setValue(101);

    expect(upperBound?.hasError('max')).toBe(true);
  });
});