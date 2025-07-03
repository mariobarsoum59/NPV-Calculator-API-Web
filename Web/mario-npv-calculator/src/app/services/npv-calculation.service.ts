import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { NPVCalculationRequest } from '../models/npv-calculation-request.model';
import { NPVCalculationResult } from '../models/npv-calculation-result.model';


@Injectable({
  providedIn: 'root'
})
export class NPVCalculationService {
  private apiUrl = `${environment.apiUrl}/api/NPVCalculation`;

  constructor(private http: HttpClient) { }

  calculateNPVRange(request: NPVCalculationRequest): Observable<NPVCalculationResult> {
    return this.http.post<ApiResponse<NPVCalculationResult>>(
      `${this.apiUrl}/calculate`,
      request
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        throw new Error(response.error?.message || 'Failed to calculate NPV');
      }),
      catchError(error => {
        console.error('NPV calculation error:', error);
        return throwError(() => error);
      })
    );
  }

  validateRequest(request: NPVCalculationRequest): string[] {
    const errors: string[] = [];

    if (!request.cashFlows || request.cashFlows.length === 0) {
      errors.push('At least one cash flow is required');
    }

    if (request.lowerBound < 0 || request.lowerBound > 100) {
      errors.push('Lower bound must be between 0 and 100');
    }

    if (request.upperBound < 0 || request.upperBound > 100) {
      errors.push('Upper bound must be between 0 and 100');
    }

    if (request.upperBound < request.lowerBound) {
      errors.push('Upper bound must be greater than or equal to lower bound');
    }

    if (request.increment <= 0 || request.increment > 100) {
      errors.push('Increment must be positive and less than or equal to 100');
    }

    return errors;
  }
}