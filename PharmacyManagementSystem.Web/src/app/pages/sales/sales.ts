import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

interface SaleLine {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  salePrice: number;
}

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Sales</h2>
      <button class="btn btn-primary" (click)="openNewSale()">New Sale</button>
    </div>

    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Date</th>
          <th>Amount</th>
          <th>Payment</th>
        </tr>
      </thead>
      <tbody>
        @for (s of sales; track s.id) {
          <tr>
            <td>{{ s.saleDate | date:'short' }}</td>
            <td>Rs {{ s.totalAmount | number:'1.2-2' }}</td>
            <td>{{ paymentLabel(s.paymentMode) }}</td>
          </tr>
        }
      </tbody>
    </table>

    <div class="modal fade" [class.show]="showSaleModal" [style.display]="showSaleModal ? 'block' : 'none'" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">New Sale</h5>
            <button type="button" class="btn-close" (click)="closeSaleModal()"></button>
          </div>
          <div class="modal-body">
            <div class="row mb-2">
              <div class="col-md-6">
                <label class="form-label">Branch</label>
                <select class="form-select" [(ngModel)]="saleForm.branchId" required>
                  <option value="">-- Select --</option>
                  @for (b of branches; track b.id) {
                    <option [value]="b.id">{{ b.name }}</option>
                  }
                </select>
              </div>
              <div class="col-md-6">
                <label class="form-label">Customer</label>
                <select class="form-select" [(ngModel)]="saleForm.customerId">
                  <option value="">Walk-in</option>
                  @for (c of customers; track c.id) {
                    <option [value]="c.id">{{ c.name }}</option>
                  }
                </select>
              </div>
            </div>
            <div class="row mb-2">
              <div class="col-md-6">
                <label class="form-label">Add by Barcode</label>
                <div class="input-group">
                  <input type="text" class="form-control" [(ngModel)]="barcodeInput" placeholder="Scan or enter barcode" (keyup.enter)="addByBarcode()" />
                  <button class="btn btn-outline-secondary" (click)="addByBarcode()">Add</button>
                </div>
              </div>
              <div class="col-md-6">
                <label class="form-label">Search Product</label>
                <select class="form-select" [(ngModel)]="productSearchId" (change)="addProduct(productSearchId)">
                  <option value="">-- Select product --</option>
                  @for (p of products; track p.id) {
                    <option [value]="p.id">{{ p.name }} - Rs {{ p.salePrice }}</option>
                  }
                </select>
              </div>
            </div>
            <div class="mb-2">
              <label class="form-label">Payment Mode</label>
              <select class="form-select" [(ngModel)]="saleForm.paymentMode" style="max-width: 150px;">
                <option [value]="0">Cash</option>
                <option [value]="1">Card</option>
                <option [value]="2">Online</option>
              </select>
            </div>
            <div class="mb-2">
              <label class="form-label">Customer CNIC (for Schedule H)</label>
              <input type="text" class="form-control" [(ngModel)]="saleForm.customerCNIC" placeholder="Required for controlled substances" style="max-width: 250px;" />
            </div>
            <table class="table table-sm">
              <thead><tr><th>Product</th><th>Qty</th><th>Price</th><th>Total</th><th></th></tr></thead>
              <tbody>
                @for (line of saleLines; track line.productId) {
                  <tr>
                    <td>{{ line.productName }}</td>
                    <td>
                      <input type="number" class="form-control form-control-sm" [(ngModel)]="line.quantity" min="1" style="width: 70px;" />
                    </td>
                    <td>Rs {{ line.unitPrice | number:'1.2-2' }}</td>
                    <td>Rs {{ (line.quantity * line.unitPrice) | number:'1.2-2' }}</td>
                    <td><button class="btn btn-sm btn-outline-danger" (click)="removeLine(line)">Remove</button></td>
                  </tr>
                }
              </tbody>
            </table>
            <p class="fw-bold">Total: Rs {{ saleTotal | number:'1.2-2' }}</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeSaleModal()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="submitSale()" [disabled]="saleLines.length === 0 || !saleForm.branchId">Complete Sale</button>
          </div>
        </div>
      </div>
      @if (showSaleModal) {
        <div class="modal-backdrop fade show"></div>
      }
    </div>
  `
})
export class SalesComponent implements OnInit {
  sales: { id: string; saleDate: string; totalAmount: number; paymentMode: number }[] = [];
  branches: { id: string; name: string }[] = [];
  customers: { id: string; name: string }[] = [];
  products: { id: string; name: string; salePrice: number }[] = [];
  showSaleModal = false;
  barcodeInput = '';
  productSearchId = '';
  saleForm = {
    branchId: '',
    customerId: '',
    paymentMode: 0,
    customerCNIC: '',
    discountAmount: 0,
    discountPercent: 0
  };
  saleLines: SaleLine[] = [];

  constructor(private api: ApiService, private auth: AuthService) {}

  ngOnInit() {
    this.loadSales();
    this.api.get<{ id: string; name: string }[]>('/branches').subscribe((b) => {
      this.branches = b || [];
      const bid = this.auth.user()?.branchId;
      if (bid && this.branches.length) this.saleForm.branchId = bid;
    });
    this.api.get<{ id: string; name: string }[]>('/customers').subscribe((c) => (this.customers = c || []));
    this.api.get<{ id: string; name: string; salePrice: number }[]>('/products').subscribe((p) => (this.products = p || []));
  }

  get saleTotal() {
    return this.saleLines.reduce((sum, l) => sum + l.quantity * l.unitPrice, 0) - this.saleForm.discountAmount;
  }

  paymentLabel(m: number) {
    return ['Cash', 'Card', 'Online'][m] ?? m;
  }

  loadSales() {
    this.api.get<{ id: string; saleDate: string; totalAmount: number; paymentMode: number }[]>('/sales').subscribe((s) => (this.sales = s || []));
  }

  openNewSale() {
    this.saleLines = [];
    this.saleForm = {
      branchId: this.saleForm.branchId || this.branches[0]?.id || '',
      customerId: '',
      paymentMode: 0,
      customerCNIC: '',
      discountAmount: 0,
      discountPercent: 0
    };
    this.barcodeInput = '';
    this.productSearchId = '';
    this.showSaleModal = true;
  }

  closeSaleModal() {
    this.showSaleModal = false;
  }

  addByBarcode() {
    if (!this.barcodeInput.trim()) return;
    this.api.get<{ id: string; name: string; salePrice: number }>(`/products/search?barcode=${encodeURIComponent(this.barcodeInput.trim())}`).subscribe({
      next: (p) => this.addProduct(p.id, p.salePrice),
      error: () => alert('Product not found')
    });
    this.barcodeInput = '';
  }

  addProduct(productId: string, price?: number) {
    if (!productId) return;
    const existing = this.saleLines.find((l) => l.productId === productId);
    if (existing) {
      existing.quantity++;
      return;
    }
    const p = this.products.find((x) => x.id === productId);
    const productName = p?.name ?? 'Product';
    const salePrice = price ?? p?.salePrice ?? 0;
    this.saleLines.push({ productId, productName, quantity: 1, unitPrice: salePrice, salePrice });
    this.productSearchId = '';
  }

  removeLine(line: SaleLine) {
    this.saleLines = this.saleLines.filter((l) => l.productId !== line.productId);
  }

  submitSale() {
    if (!this.saleForm.branchId || this.saleLines.length === 0) return;
    const body = {
      branchId: this.saleForm.branchId,
      customerId: this.saleForm.customerId || null,
      paymentMode: this.saleForm.paymentMode,
      discountAmount: this.saleForm.discountAmount,
      discountPercent: this.saleForm.discountPercent,
      customerCNIC: this.saleForm.customerCNIC || null,
      lines: this.saleLines.map((l) => ({ productId: l.productId, quantity: l.quantity, unitPrice: l.unitPrice }))
    };
    this.api.post<{ id: string; totalAmount: number }>('/sales', body).subscribe({
      next: (r) => {
        alert('Sale completed. Total: Rs ' + (r?.totalAmount ?? this.saleTotal).toFixed(2));
        this.closeSaleModal();
        this.loadSales();
      },
      error: (e) => alert(e.error?.message || 'Sale failed')
    });
  }
}
