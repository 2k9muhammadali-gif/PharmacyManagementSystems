import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface Product {
  id: string;
  name: string;
  genericName?: string;
  strength?: string;
  formulation?: string;
  schedule: number;
  barcode?: string;
  salePrice: number;
  reorderPoint: number;
  isActive: boolean;
  manufacturerId: string;
  manufacturerName?: string;
}

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Products</h2>
      <button class="btn btn-primary" (click)="openForm()">Add Product</button>
    </div>
    <div class="mb-3">
      <input type="text" class="form-control" placeholder="Search..." [(ngModel)]="search" (input)="load()" style="max-width: 300px;" />
    </div>
    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Name</th>
          <th>Manufacturer</th>
          <th>Barcode</th>
          <th>Price</th>
          <th>Schedule</th>
          <th>Status</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (p of products; track p.id) {
          <tr>
            <td>{{ p.name }}</td>
            <td>{{ p.manufacturerName }}</td>
            <td>{{ p.barcode || '-' }}</td>
            <td>Rs {{ p.salePrice | number:'1.2-2' }}</td>
            <td>{{ scheduleLabel(p.schedule) }}</td>
            <td><span class="badge" [class.bg-success]="p.isActive" [class.bg-secondary]="!p.isActive">{{ p.isActive ? 'Active' : 'Inactive' }}</span></td>
            <td>
              <button type="button" class="btn btn-sm btn-outline-primary me-1" (click)="edit(p); $event.stopPropagation()">Edit</button>
              @if (p.isActive) {
                <button class="btn btn-sm btn-outline-danger" (click)="remove(p)">Deactivate</button>
              }
            </td>
          </tr>
        }
      </tbody>
    </table>

    <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showModal) {
        <div class="modal-backdrop fade show" (click)="closeForm()"></div>
      }
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Edit Product' : 'Add Product' }}</h5>
            <button type="button" class="btn-close" (click)="closeForm()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" required />
            </div>
            <div class="mb-2">
              <label class="form-label">Manufacturer *</label>
              <select class="form-select" [(ngModel)]="form.manufacturerId" required>
                <option value="">-- Select --</option>
                @for (m of manufacturers; track m.id) {
                  <option [value]="m.id">{{ m.name }}</option>
                }
              </select>
            </div>
            <div class="row">
              <div class="col-md-6 mb-2">
                <label class="form-label">Generic Name</label>
                <input type="text" class="form-control" [(ngModel)]="form.genericName" />
              </div>
              <div class="col-md-6 mb-2">
                <label class="form-label">Strength</label>
                <input type="text" class="form-control" [(ngModel)]="form.strength" />
              </div>
            </div>
            <div class="row">
              <div class="col-md-6 mb-2">
                <label class="form-label">Schedule</label>
                <select class="form-select" [(ngModel)]="form.schedule">
                  <option [value]="0">OTC</option>
                  <option [value]="1">Prescription</option>
                  <option [value]="2">Schedule H</option>
                </select>
              </div>
              <div class="col-md-6 mb-2">
                <label class="form-label">Sale Price (Rs) *</label>
                <input type="number" step="0.01" class="form-control" [(ngModel)]="form.salePrice" required />
              </div>
            </div>
            <div class="row">
              <div class="col-md-6 mb-2">
                <label class="form-label">Barcode</label>
                <input type="text" class="form-control" [(ngModel)]="form.barcode" />
              </div>
              <div class="col-md-6 mb-2">
                <label class="form-label">Reorder Point</label>
                <input type="number" class="form-control" [(ngModel)]="form.reorderPoint" />
              </div>
            </div>
            @if (editingId) {
              <div class="mb-2">
                <label class="form-check-label">
                  <input type="checkbox" class="form-check-input" [(ngModel)]="form.isActive" />
                  Active
                </label>
              </div>
            }
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeForm()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="save()">Save</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  manufacturers: { id: string; name: string }[] = [];
  search = '';
  showModal = false;
  editingId: string | null = null;
  form: Partial<Product> & { manufacturerId?: string; reorderPoint?: number } = {
    name: '',
    manufacturerId: '',
    genericName: '',
    strength: '',
    formulation: '',
    schedule: 0,
    barcode: '',
    salePrice: 0,
    reorderPoint: 0,
    isActive: true
  };

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.load();
    this.api.get<{ id: string; name: string }[]>('/manufacturers').subscribe((m) => (this.manufacturers = m || []));
  }

  load() {
    const params: Record<string, string> = {};
    if (this.search) params['search'] = this.search;
    this.api.get<Product[]>('/products', params).subscribe((p) => {
      this.products = p || [];
      this.cdr.detectChanges();
    });
  }

  scheduleLabel(s: number) {
    return ['OTC', 'Prescription', 'Schedule H'][s] ?? s;
  }

  openForm() {
    this.editingId = null;
    this.form = { name: '', manufacturerId: '', genericName: '', strength: '', formulation: '', schedule: 0, barcode: '', salePrice: 0, reorderPoint: 0, isActive: true };
    this.showModal = true;
    document.body.classList.add('modal-open');
  }

  edit(p: Product) {
    this.editingId = p.id;
    this.form = {
      id: p.id,
      name: p.name,
      genericName: p.genericName,
      strength: p.strength,
      formulation: p.formulation,
      schedule: p.schedule,
      barcode: p.barcode,
      salePrice: p.salePrice,
      reorderPoint: p.reorderPoint ?? 0,
      isActive: p.isActive,
      manufacturerId: p.manufacturerId
    };
    this.showModal = true;
    document.body.classList.add('modal-open');
    this.cdr.detectChanges();
  }

  closeForm() {
    this.showModal = false;
    document.body.classList.remove('modal-open');
  }

  save() {
    if (!this.form.name || !this.form.manufacturerId) return;
    const body: Record<string, unknown> = {
      manufacturerId: this.form.manufacturerId,
      name: this.form.name,
      genericName: this.form.genericName ?? null,
      strength: this.form.strength ?? null,
      formulation: this.form.formulation ?? null,
      schedule: this.form.schedule ?? 0,
      barcode: this.form.barcode ?? null,
      reorderPoint: this.form.reorderPoint ?? 0,
      salePrice: this.form.salePrice ?? 0,
      isActive: this.form.isActive ?? true
    };
    if (this.editingId) {
      body['id'] = this.editingId;
      this.api.put(`/products/${this.editingId}`, body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/products', body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }

  remove(p: Product) {
    if (!confirm('Deactivate this product?')) return;
    this.api.delete(`/products/${p.id}`).subscribe({
      next: () => this.load(),
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }
}
