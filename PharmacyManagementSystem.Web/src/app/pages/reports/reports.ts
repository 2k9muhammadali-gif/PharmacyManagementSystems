import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Reports</h2>

    <div class="row mb-4">
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Sales Report</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">From</label>
              <input type="date" class="form-control" [(ngModel)]="salesFrom" />
            </div>
            <div class="mb-2">
              <label class="form-label">To</label>
              <input type="date" class="form-control" [(ngModel)]="salesTo" />
            </div>
            <button class="btn btn-primary" (click)="loadSalesReport()">Load</button>
            @if (salesReport) {
              <div class="mt-3">
                <p><strong>Total Sales:</strong> Rs {{ salesReport.total | number:'1.2-2' }}</p>
                <p><strong>Transaction Count:</strong> {{ salesReport.count }}</p>
              </div>
            }
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Top Products</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">Top N</label>
              <input type="number" class="form-control" [(ngModel)]="topN" style="max-width: 80px;" />
            </div>
            <button class="btn btn-primary" (click)="loadTopProducts()">Load</button>
            @if (topProducts.length) {
              <ul class="list-group mt-3">
                @for (p of topProducts; track p.productId) {
                  <li class="list-group-item d-flex justify-content-between">
                    <span>{{ p.productName }}</span>
                    <span>Qty: {{ p.quantity }} | Rs {{ p.revenue | number:'1.2-2' }}</span>
                  </li>
                }
              </ul>
            }
          </div>
        </div>
      </div>
    </div>

    <div class="row">
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Stock Expiring Soon</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">Branch</label>
              <select class="form-select" [(ngModel)]="expiryBranchId">
                <option value="">-- Select --</option>
                @for (b of branches; track b.id) {
                  <option [value]="b.id">{{ b.name }}</option>
                }
              </select>
            </div>
            <div class="mb-2">
              <label class="form-label">Within Days</label>
              <input type="number" class="form-control" [(ngModel)]="expiryDays" style="max-width: 100px;" />
            </div>
            <button class="btn btn-primary" (click)="loadExpiry()" [disabled]="!expiryBranchId">Load</button>
            @if (expiryItems.length) {
              <table class="table table-sm mt-3">
                <thead><tr><th>Product</th><th>Batch</th><th>Qty</th><th>Expiry</th></tr></thead>
                <tbody>
                  @for (e of expiryItems; track e.batchNumber) {
                    <tr>
                      <td>{{ e.productName }}</td>
                      <td>{{ e.batchNumber }}</td>
                      <td>{{ e.quantity }}</td>
                      <td>{{ e.expiryDate | date:'shortDate' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Stock Valuation</div>
          <div class="card-body">
            <div class="mb-2">
              <label class="form-label">Branch</label>
              <select class="form-select" [(ngModel)]="valuationBranchId">
                <option value="">-- Select --</option>
                @for (b of branches; track b.id) {
                  <option [value]="b.id">{{ b.name }}</option>
                }
              </select>
            </div>
            <button class="btn btn-primary" (click)="loadValuation()" [disabled]="!valuationBranchId">Load</button>
            @if (stockValuation) {
              <div class="mt-3">
                <p><strong>Total Value:</strong> Rs {{ stockValuation.totalValue | number:'1.2-2' }}</p>
                <p><strong>Total Items:</strong> {{ stockValuation.totalItems }}</p>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class ReportsComponent implements OnInit {
  branches: { id: string; name: string }[] = [];
  salesFrom = new Date().toISOString().slice(0, 10);
  salesTo = new Date().toISOString().slice(0, 10);
  salesReport: { total: number; count: number } | null = null;
  topN = 10;
  topProducts: { productId: string; productName: string; quantity: number; revenue: number }[] = [];
  expiryBranchId = '';
  expiryDays = 30;
  expiryItems: { productName: string; batchNumber: string; quantity: number; expiryDate: string }[] = [];
  valuationBranchId = '';
  stockValuation: { totalValue: number; totalItems: number } | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.get<{ id: string; name: string }[]>('/branches').subscribe((b) => {
      this.branches = b || [];
      if (this.branches.length && !this.expiryBranchId) this.expiryBranchId = this.branches[0].id;
      if (this.branches.length && !this.valuationBranchId) this.valuationBranchId = this.branches[0].id;
    });
  }

  loadSalesReport() {
    const params: Record<string, string> = {
      from: this.salesFrom + 'T00:00:00',
      to: this.salesTo + 'T23:59:59'
    };
    this.api.get<{ total: number; count: number }>('/reports/sales', params).subscribe({
      next: (r) => (this.salesReport = r),
      error: () => (this.salesReport = null)
    });
  }

  loadTopProducts() {
    const params: Record<string, string> = { top: String(this.topN) };
    this.api.get<{ productId: string; productName: string; quantity: number; revenue: number }[]>('/reports/top-products', params).subscribe({
      next: (p) => (this.topProducts = p || []),
      error: () => (this.topProducts = [])
    });
  }

  loadExpiry() {
    if (!this.expiryBranchId) return;
    this.api.get<{ productName: string; batchNumber: string; quantity: number; expiryDate: string }[]>(
      '/reports/expiry',
      { branchId: this.expiryBranchId, days: String(this.expiryDays) }
    ).subscribe({
      next: (e) => (this.expiryItems = e || []),
      error: () => (this.expiryItems = [])
    });
  }

  loadValuation() {
    if (!this.valuationBranchId) return;
    this.api.get<{ totalValue: number; totalItems: number }>(
      '/reports/stock-valuation',
      { branchId: this.valuationBranchId }
    ).subscribe({
      next: (v) => (this.stockValuation = v),
      error: () => (this.stockValuation = null)
    });
  }
}
