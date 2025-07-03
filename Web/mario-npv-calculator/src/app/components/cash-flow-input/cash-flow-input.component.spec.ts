import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormArray, FormControl, ReactiveFormsModule } from '@angular/forms';
import { CashFlowInputComponent } from './cash-flow-input.component';

describe('CashFlowInputComponent', () => {
  let component: CashFlowInputComponent;
  let fixture: ComponentFixture<CashFlowInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CashFlowInputComponent],
      imports: [ReactiveFormsModule]
    })
      .compileComponents();

    fixture = TestBed.createComponent(CashFlowInputComponent);
    component = fixture.componentInstance;

    // Setup test FormArray
    component.cashFlows = new FormArray([
      new FormControl(-1000),
      new FormControl(500)
    ]);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should get control at index', () => {
    const control = component.getControlAt(0);
    expect(control.value).toBe(-1000);
  });

  it('should get all form controls', () => {
    const controls = component.formControls;
    expect(controls.length).toBe(2);
  });

  it('should emit addCashFlow event', () => {
    spyOn(component.addCashFlow, 'emit');

    component.onAddCashFlow();

    expect(component.addCashFlow.emit).toHaveBeenCalled();
  });

  it('should emit removeCashFlow event with index', () => {
    spyOn(component.removeCashFlow, 'emit');

    component.onRemoveCashFlow(1);

    expect(component.removeCashFlow.emit).toHaveBeenCalledWith(1);
  });

  it('should emit updateCashFlow event with index and value', () => {
    spyOn(component.updateCashFlow, 'emit');
    const mockEvent = { target: { value: '1000' } } as any;

    component.onUpdateCashFlow(0, mockEvent);

    expect(component.updateCashFlow.emit).toHaveBeenCalledWith({
      index: 0,
      value: '1000'
    });
  });

  it('should track by index', () => {
    expect(component.trackByIndex(5)).toBe(5);
  });

  it('should render input fields', () => {
    const compiled = fixture.nativeElement;
    const inputs = compiled.querySelectorAll('input[type="number"]');
    expect(inputs.length).toBe(2);
  });

  it('should render add button', () => {
    const compiled = fixture.nativeElement;
    const addButton = compiled.querySelector('.add-btn');
    expect(addButton).toBeTruthy();
  });

  it('should render remove buttons', () => {
    const compiled = fixture.nativeElement;
    const removeButtons = compiled.querySelectorAll('.remove-btn');
    expect(removeButtons.length).toBe(2);
  });

  it('should disable remove button when only one item', () => {
    component.cashFlows = new FormArray([new FormControl(100)]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    const removeButton = compiled.querySelector('.remove-btn');
    expect(removeButton.disabled).toBe(true);
  });

  it('should call onAddCashFlow when add button clicked', () => {
    spyOn(component, 'onAddCashFlow');
    const compiled = fixture.nativeElement;
    const addButton = compiled.querySelector('.add-btn');

    addButton.click();

    expect(component.onAddCashFlow).toHaveBeenCalled();
  });

  it('should call onRemoveCashFlow when remove button clicked', () => {
    spyOn(component, 'onRemoveCashFlow');
    const compiled = fixture.nativeElement;
    const removeButtons = compiled.querySelectorAll('.remove-btn');

    removeButtons[0].click();

    expect(component.onRemoveCashFlow).toHaveBeenCalledWith(0);
  });

  it('should display correct period labels', () => {
    const compiled = fixture.nativeElement;
    const labels = compiled.querySelectorAll('.period-label');

    expect(labels[0].textContent).toContain('Period 0');
    expect(labels[1].textContent).toContain('Period 1');
  });

  it('should show validation error message', () => {
    component.cashFlows.at(0).setErrors({ required: true });
    component.cashFlows.at(0).markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    const errorMessage = compiled.querySelector('.error-message');
    expect(errorMessage).toBeTruthy();
  });

  it('should apply invalid class to invalid input', () => {
    component.cashFlows.at(0).setErrors({ required: true });
    component.cashFlows.at(0).markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    const input = compiled.querySelector('input[type="number"]');
    expect(input.classList.contains('invalid')).toBe(true);
  });
});