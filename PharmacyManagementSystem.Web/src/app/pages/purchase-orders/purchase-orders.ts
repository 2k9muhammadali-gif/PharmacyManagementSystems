import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

interface PurchaseOrderLine {
  productId: string;
  productName: string;
  formulation?: string;
  manufacturerId: string;
  manufacturerName: string;
  quantity: number;
  unitPrice: number;
}

interface POLineInput {
  lineId: string;
  productId: string;
  productName: string;
  formulation?: string;
  manufacturerId: string;
  manufacturerName: string;
  quantity: number;
  unitPrice: number;
}

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Purchase Orders</h2>
      <button class="btn btn-primary" (click)="openCreateModal()">New Purchase Order</button>
    </div>

    <div class="mb-3">
      <label class="form-label">Branch</label>
      <select class="form-select" [(ngModel)]="filterBranchId" (change)="load()" style="max-width: 300px;">
        <option value="">All branches</option>
        @for (b of branches; track b.id) {
          <option [value]="b.id">{{ b.name }}</option>
        }
      </select>
    </div>

    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Date</th>
          <th>Branch</th>
          <th>Distribution</th>
          <th>Amount</th>
          <th>Status</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (po of purchaseOrders; track po.id) {
          <tr>
            <td>{{ po.orderDate | date:'short' }}</td>
            <td>{{ getBranchName(po.branchId) }}</td>
            <td>{{ po.distributionName }}</td>
            <td>Rs {{ po.totalAmount | number:'1.2-2' }}</td>
            <td><span class="badge" [class.bg-success]="po.status === 2" [class.bg-warning]="po.status === 1" [class.bg-secondary]="po.status === 0">{{ statusLabel(po.status) }}</span></td>
            <td>
              <button type="button" class="btn btn-sm btn-outline-primary me-1" (click)="viewPO(po)">View</button>
              @if (po.status === 1) {
                <button class="btn btn-sm btn-success" (click)="receive(po)">Receive</button>
              }
            </td>
          </tr>
        }
      </tbody>
    </table>

    <!-- Create PO modal -->
    <div class="modal fade" [class.show]="showCreateModal" [style.display]="showCreateModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showCreateModal) {
        <div class="modal-backdrop fade show" (click)="closeCreateModal()"></div>
      }
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">New Purchase Order</h5>
            <button type="button" class="btn-close" (click)="closeCreateModal()"></button>
          </div>
          <div class="modal-body">
            <div class="row mb-3">
              <div class="col-md-6">
                <label class="form-label">Branch *</label>
                <select class="form-select" [(ngModel)]="poForm.branchId" (change)="onBranchChange()" required>
                  <option value="">-- Select --</option>
                  @for (b of branches; track b.id) {
                    <option [value]="b.id">{{ b.name }}</option>
                  }
                </select>
              </div>
              <div class="col-md-6">
                <label class="form-label">Distribution *</label>
                <select class="form-select" [(ngModel)]="poForm.distributionId" (change)="onDistributionChange()" required>
                  <option value="">-- Select --</option>
                  @for (d of distributions; track d.id) {
                    <option [value]="d.id">{{ d.name }}</option>
                  }
                </select>
              </div>
            </div>
            <hr />
            <h6>Order Lines</h6>
            <div class="mb-2">
              <label class="form-label">Add Line</label>
              <div class="d-flex gap-2 flex-wrap">
                <select class="form-select col" [(ngModel)]="newLineProductId" (change)="onProductSelect()" style="max-width: 250px;">
                  <option value="">-- Product --</option>
                  @for (p of products; track p.id) {
                    <option [value]="p.id">{{ p.name }}</option>
                  }
                </select>
                <select class="form-select col" [(ngModel)]="newLineManufacturerId" style="max-width: 200px;">
                  <option value="">-- Company --</option>
                  @for (c of distributionCompanies; track c.manufacturerId) {
                    <option [value]="c.manufacturerId">{{ c.manufacturerName }}</option>
                  }
                </select>
                <input type="number" class="form-control" [(ngModel)]="newLineQty" placeholder="Qty" min="1" style="width: 80px;" />
                <input type="number" step="0.01" class="form-control" [(ngModel)]="newLinePrice" placeholder="Price" style="width: 100px;" />
                <button class="btn btn-primary" (click)="addLine()" [disabled]="!canAddLine()">Add</button>
              </div>
            </div>
            <table class="table table-sm">
              <thead><tr><th>Product</th><th>Form</th><th>Company</th><th>Qty</th><th>Unit Price</th><th>Total</th><th></th></tr></thead>
              <tbody>
                @for (line of poLines; track line.lineId) {
                  <tr>
                    <td>{{ line.productName }}</td>
                    <td>{{ line.formulation || '-' }}</td>
                    <td>{{ line.manufacturerName }}</td>
                    <td>{{ line.quantity }}</td>
                    <td>Rs {{ line.unitPrice | number:'1.2-2' }}</td>
                    <td>Rs {{ (line.quantity * line.unitPrice) | number:'1.2-2' }}</td>
                    <td><button class="btn btn-sm btn-outline-danger" (click)="removeLine(line)">Remove</button></td>
                  </tr>
                }
                @if (poLines.length === 0) {
                  <tr><td colspan="7" class="text-muted">No lines added yet.</td></tr>
                }
              </tbody>
            </table>
            <p class="fw-bold">Total: Rs {{ poTotal | number:'1.2-2' }}</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeCreateModal()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="submitPO()" [disabled]="poLines.length === 0 || !poForm.branchId || !poForm.distributionId">Submit Order</button>
          </div>
        </div>
      </div>
    </div>

    <!-- View PO modal -->
    <div class="modal fade" [class.show]="showViewModal" [style.display]="showViewModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showViewModal) {
        <div class="modal-backdrop fade show" (click)="closeViewModal()"></div>
      }
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Purchase Order - {{ viewedPO?.distributionName }}</h5>
            <button type="button" class="btn-close" (click)="closeViewModal()"></button>
          </div>
          <div class="modal-body">
            @if (viewedPO) {
              <p><strong>Date:</strong> {{ viewedPO.orderDate | date:'short' }} | <strong>Status:</strong> {{ statusLabel(viewedPO.status) }} | <strong>Total:</strong> Rs {{ viewedPO.totalAmount | number:'1.2-2' }}</p>
              <table class="table table-sm">
                <thead><tr><th>Product</th><th>Form</th><th>Company</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr></thead>
                <tbody>
                  @for (l of viewedPOLines; track l.productId) {
                    <tr>
                      <td>{{ l.productName }}</td>
                      <td>{{ l.formulation || '-' }}</td>
                      <td>{{ l.manufacturerName }}</td>
                      <td>{{ l.quantity }}</td>
                      <td>Rs {{ l.unitPrice | number:'1.2-2' }}</td>
                      <td>Rs {{ (l.quantity * l.unitPrice) | number:'1.2-2' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeViewModal()">Close</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PurchaseOrdersComponent implements OnInit {
  purchaseOrders: { id: string; branchId: string; distributionId: string; distributionName: string; orderDate: string; status: number; totalAmount: number }[] = [];
  branches: { id: string; name: string }[] = [];
  distributions: { id: string; name: string }[] = [];
  products: { id: string; name: string; manufacturerId: string; formulation?: string }[] = [];
  distributionCompanies: { manufacturerId: string; manufacturerName: string }[] = [];
  filterBranchId = '';
  showCreateModal = false;
  showViewModal = false;
  poForm = { branchId: '', distributionId: '' };
  poLines: POLineInput[] = [];
  newLineProductId = '';
  newLineManufacturerId = '';
  newLineQty = 1;
  newLinePrice = 0;
  viewedPO: { id: string; orderDate: string; status: number; totalAmount: number; distributionName: string } | null = null;
  viewedPOLines: PurchaseOrderLine[] = [];

  constructor(private api: ApiService, private auth: AuthService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.load();
    this.api.get<{ id: string; name: string }[]>('/branches').subscribe((b) => {
      this.branches = b || [];
      const bid = this.auth.user()?.branchId;
      if (bid) this.filterBranchId = bid;
    });
    this.api.get<{ id: string; name: string }[]>('/distributions').subscribe((d) => (this.distributions = d || []));
    this.api.get<{ id: string; name: string; manufacturerId: string; formulation?: string }[]>('/products').subscribe((p) => (this.products = p || []));
  }

  get poTotal() {
    return this.poLines.reduce((sum, l) => sum + l.quantity * l.unitPrice, 0);
  }

  statusLabel(s: number) {
    return ['Draft', 'Submitted', 'Received'][s] ?? s;
  }

  getBranchName(branchId: string) {
    return this.branches.find((b) => b.id === branchId)?.name ?? branchId;
  }

  load() {
    const params: Record<string, string> = {};
    if (this.filterBranchId) params['branchId'] = this.filterBranchId;
    this.api.get<typeof this.purchaseOrders>('/purchaseorders', params).subscribe((po) => {
      this.purchaseOrders = po || [];
      this.cdr.detectChanges();
    });
  }

  onBranchChange() {
    // Reset distribution if needed
  }

  onDistributionChange() {
    this.distributionCompanies = [];
    if (!this.poForm.distributionId) return;
    this.api.get<{ manufacturerId: string; manufacturerName: string }[]>(`/distributions/${this.poForm.distributionId}/companies`).subscribe((c) => {
      this.distributionCompanies = c || [];
      this.newLineManufacturerId = '';
      this.cdr.detectChanges();
    });
  }

  onProductSelect() {
    const p = this.products.find((x) => x.id === this.newLineProductId);
    if (p && this.distributionCompanies.some((c) => c.manufacturerId === p.manufacturerId)) {
      this.newLineManufacturerId = p.manufacturerId;
    } else {
      this.newLineManufacturerId = this.distributionCompanies[0]?.manufacturerId ?? '';
    }
    this.cdr.detectChanges();
  }

  canAddLine() {
    return this.newLineProductId && this.newLineManufacturerId && this.newLineQty >= 1 && this.newLinePrice >= 0;
  }

  addLine() {
    if (!this.canAddLine()) return;
    const p = this.products.find((x) => x.id === this.newLineProductId);
    const c = this.distributionCompanies.find((x) => x.manufacturerId === this.newLineManufacturerId);
    if (!p || !c) return;
    this.poLines.push({
      lineId: crypto.randomUUID(),
      productId: p.id,
      productName: p.name,
      formulation: p.formulation,
      manufacturerId: c.manufacturerId,
      manufacturerName: c.manufacturerName,
      quantity: this.newLineQty,
      unitPrice: this.newLinePrice
    });
    this.newLineProductId = '';
    this.newLineManufacturerId = '';
    this.newLineQty = 1;
    this.newLinePrice = 0;
    this.cdr.detectChanges();
  }

  removeLine(line: POLineInput) {
    this.poLines = this.poLines.filter((l) => l.lineId !== line.lineId);
  }

  openCreateModal() {
    this.poForm = { branchId: this.auth.user()?.branchId ?? this.branches[0]?.id ?? '', distributionId: '' };
    this.poLines = [];
    this.distributionCompanies = [];
    this.newLineProductId = '';
    this.newLineManufacturerId = '';
    this.newLineQty = 1;
    this.newLinePrice = 0;
    this.showCreateModal = true;
    document.body.classList.add('modal-open');
  }

  closeCreateModal() {
    this.showCreateModal = false;
    document.body.classList.remove('modal-open');
    this.load();
  }

  submitPO() {
    if (!this.poForm.branchId || !this.poForm.distributionId || this.poLines.length === 0) return;
    const body = {
      branchId: this.poForm.branchId,
      distributionId: this.poForm.distributionId,
      lines: this.poLines.map((l) => ({ productId: l.productId, manufacturerId: l.manufacturerId, quantity: l.quantity, unitPrice: l.unitPrice }))
    };
    this.api.post('/purchaseorders', body).subscribe({
      next: () => {
        alert('Purchase order submitted.');
        this.closeCreateModal();
      },
      error: (e) => alert(e.error?.message || 'Failed to create order')
    });
  }

  viewPO(po: { id: string }) {
    this.api.get<{ orderDate: string; status: number; totalAmount: number; distributionName: string; lines: PurchaseOrderLine[] }>(`/purchaseorders/${po.id}`).subscribe({
      next: (data) => {
        this.viewedPO = { ...data, id: po.id };
        this.viewedPOLines = data?.lines ?? [];
        this.showViewModal = true;
        document.body.classList.add('modal-open');
        this.cdr.detectChanges();
      },
      error: (e) => alert(e.error?.message || 'Failed to load')
    });
  }

  closeViewModal() {
    this.showViewModal = false;
    this.viewedPO = null;
    this.viewedPOLines = [];
    document.body.classList.remove('modal-open');
  }

  receive(po: { id: string }) {
    if (!confirm('Mark this order as received? Stock will be updated.')) return;
    this.api.post(`/purchaseorders/${po.id}/receive`, {}).subscribe({
      next: () => {
        alert('Goods received.');
        this.load();
      },
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }
}
