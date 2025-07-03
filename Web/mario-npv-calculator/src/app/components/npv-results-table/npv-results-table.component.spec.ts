import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NPVResultItem } from 'src/app/models/npv-result-item.model';
import { NPVResultsTableComponent } from './npv-results-table.component';

describe('NPVResultsTableComponent', () => {
  let component: NPVResultsTableComponent;
  let fixture: ComponentFixture<NPVResultsTableComponent>;
  let compiled: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NPVResultsTableComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(NPVResultsTableComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty results array', () => {
    expect(component.results).toEqual([]);
  });

  it('should accept results input', () => {
    const testResults: NPVResultItem[] = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' },
      { discountRate: 0.10, npv: -50, formattedRate: '10%', currency: 'USD' }
    ];

    component.results = testResults;

    expect(component.results.length).toBe(2);
    expect(component.results).toEqual(testResults);
  });

  it('should return positive class for positive NPV', () => {
    const result = component.getNPVClass(100);
    expect(result).toBe('positive');
  });

  it('should return positive class for zero NPV', () => {
    const result = component.getNPVClass(0);
    expect(result).toBe('positive');
  });

  it('should return negative class for negative NPV', () => {
    const result = component.getNPVClass(-100);
    expect(result).toBe('negative');
  });

  it('should format currency correctly', () => {
    const result = component.formatCurrency(123.456, 'USD');
    expect(result).toBe('USD 123.46');
  });

  it('should format currency with two decimal places', () => {
    const result = component.formatCurrency(100, 'EUR');
    expect(result).toBe('EUR 100.00');
  });

  it('should format negative currency correctly', () => {
    const result = component.formatCurrency(-50.5, 'GBP');
    expect(result).toBe('GBP -50.50');
  });

  it('should render table when results exist', () => {
    component.results = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const table = compiled.querySelector('table');
    expect(table).toBeTruthy();
  });

  it('should render correct number of rows', () => {
    component.results = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' },
      { discountRate: 0.10, npv: 50, formattedRate: '10%', currency: 'USD' },
      { discountRate: 0.15, npv: -25, formattedRate: '15%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const rows = compiled.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);
  });

  it('should display formatted rate in table', () => {
    component.results = [
      { discountRate: 0.05, npv: 100, formattedRate: '5.00%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const rateCell = compiled.querySelector('.rate-cell');
    expect(rateCell?.textContent).toContain('5.00%');
  });

  it('should apply positive class to positive NPV cells', () => {
    component.results = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const npvCell = compiled.querySelector('.npv-cell');
    expect(npvCell?.classList.contains('positive')).toBe(true);
  });

  it('should apply negative class to negative NPV cells', () => {
    component.results = [
      { discountRate: 0.15, npv: -100, formattedRate: '15%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const npvCell = compiled.querySelector('.npv-cell');
    expect(npvCell?.classList.contains('negative')).toBe(true);
  });

  it('should display status badge', () => {
    component.results = [
      { discountRate: 0.05, npv: 100, formattedRate: '5%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const statusBadge = compiled.querySelector('.status-badge');
    expect(statusBadge?.textContent).toContain('Profitable');
  });

  it('should display loss status for negative NPV', () => {
    component.results = [
      { discountRate: 0.15, npv: -100, formattedRate: '15%', currency: 'USD' }
    ];
    fixture.detectChanges();

    const statusBadge = compiled.querySelector('.status-badge');
    expect(statusBadge?.textContent).toContain('Loss');
  });

  it('should handle empty results array', () => {
    component.results = [];
    fixture.detectChanges();

    const rows = compiled.querySelectorAll('tbody tr');
    expect(rows.length).toBe(0);
  });

  it('should format large numbers correctly', () => {
    const result = component.formatCurrency(1234567.89, 'USD');
    expect(result).toBe('USD 1234567.89');
  });

  it('should handle different currencies', () => {
    const currencies = ['USD', 'EUR', 'GBP', 'JPY', 'CHF'];

    currencies.forEach(currency => {
      const result = component.formatCurrency(100, currency);
      expect(result).toBe(`${currency} 100.00`);
    });
  });
});