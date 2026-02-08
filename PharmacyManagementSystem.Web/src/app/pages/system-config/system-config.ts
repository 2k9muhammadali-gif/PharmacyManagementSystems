import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface ProductForm {
  id: string;
  name: string;
  displayOrder: number;
  isActive: boolean;
}

@Component({
  selector: 'app-system-config',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>System Configuration</h2>

    <ul class="nav nav-tabs mb-3">
      <li class="nav-item">
        <a class="nav-link" [class.active]="activeTab === 'forms'" (click)="activeTab = 'forms'" href="javascript:void(0)">Product Forms</a>
      </li>
      <li class="nav-item">
        <span class="nav-link disabled text-muted">More settings (coming soon)</span>
      </li>
    </ul>

    @if (activeTab === 'forms') {
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5 class="mb-0">Product Forms</h5>
          <button class="btn btn-primary btn-sm" (click)="openFormModal()">Add Form</button>
        </div>
        <div class="card-body">
          <p class="text-muted small">Product forms (e.g. Tablet, Injection, Capsule) appear when adding products. Add custom forms here.</p>
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>Name</th>
                <th>Order</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (f of productForms; track f.id) {
                <tr>
                  <td>{{ f.name }}</td>
                  <td>{{ f.displayOrder }}</td>
                  <td><span class="badge" [class.bg-success]="f.isActive" [class.bg-secondary]="!f.isActive">{{ f.isActive ? 'Active' : 'Inactive' }}</span></td>
                  <td>
                    <button type="button" class="btn btn-sm btn-outline-primary me-1" (click)="editForm(f)">Edit</button>
                    @if (!isFormInUse(f.id)) {
                      <button class="btn btn-sm btn-outline-danger" (click)="deleteForm(f)">Delete</button>
                    } @else {
                      <button class="btn btn-sm btn-outline-warning" (click)="toggleFormActive(f)">{{ f.isActive ? 'Deactivate' : 'Activate' }}</button>
                    }
                  </td>
                </tr>
              }
              @if (productForms.length === 0) {
                <tr><td colspan="4" class="text-muted">No product forms yet.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    <div class="modal fade" [class.show]="showFormModal" [style.display]="showFormModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showFormModal) {
        <div class="modal-backdrop fade show" (click)="closeFormModal()"></div>
      }
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingFormId ? 'Edit Product Form' : 'Add Product Form' }}</h5>
            <button type="button" class="btn-close" (click)="closeFormModal()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="formForm.name" placeholder="e.g. Tablet, Injection" required />
            </div>
            <div class="mb-2">
              <label class="form-label">Display Order</label>
              <input type="number" class="form-control" [(ngModel)]="formForm.displayOrder" />
              <small class="text-muted">Lower numbers appear first in dropdowns</small>
            </div>
            @if (editingFormId) {
              <div class="mb-2">
                <label class="form-check-label">
                  <input type="checkbox" class="form-check-input" [(ngModel)]="formForm.isActive" />
                  Active
                </label>
              </div>
            }
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeFormModal()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="saveForm()">Save</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class SystemConfigComponent implements OnInit {
  activeTab = 'forms';
  productForms: ProductForm[] = [];
  showFormModal = false;
  editingFormId: string | null = null;
  formForm: Partial<ProductForm> = { name: '', displayOrder: 0, isActive: true };
  formsInUse: Set<string> = new Set();

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadForms();
    this.loadFormsInUse();
  }

  loadForms() {
    this.api.get<ProductForm[]>('/productforms').subscribe((f) => {
      this.productForms = f || [];
      this.cdr.detectChanges();
    });
  }

  loadFormsInUse() {
    this.api.get<{ productFormId?: string }[]>('/products').subscribe((p) => {
      this.formsInUse = new Set((p || []).filter((x) => x.productFormId).map((x) => x.productFormId!));
      this.cdr.detectChanges();
    });
  }

  isFormInUse(id: string) {
    return this.formsInUse.has(id);
  }

  openFormModal() {
    this.editingFormId = null;
    this.formForm = { name: '', displayOrder: this.productForms.length, isActive: true };
    this.showFormModal = true;
    document.body.classList.add('modal-open');
  }

  editForm(f: ProductForm) {
    this.editingFormId = f.id;
    this.formForm = { id: f.id, name: f.name, displayOrder: f.displayOrder, isActive: f.isActive };
    this.showFormModal = true;
    document.body.classList.add('modal-open');
  }

  closeFormModal() {
    this.showFormModal = false;
    document.body.classList.remove('modal-open');
  }

  saveForm() {
    if (!this.formForm.name?.trim()) return;
    const body = {
      id: this.formForm.id ?? crypto.randomUUID(),
      name: this.formForm.name.trim(),
      displayOrder: this.formForm.displayOrder ?? 0,
      isActive: this.formForm.isActive ?? true
    };
    if (this.editingFormId) {
      this.api.put(`/productforms/${this.editingFormId}`, body).subscribe({
        next: () => { this.closeFormModal(); this.loadForms(); this.loadFormsInUse(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/productforms', body).subscribe({
        next: () => { this.closeFormModal(); this.loadForms(); this.loadFormsInUse(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }

  deleteForm(f: ProductForm) {
    if (!confirm('Delete "' + f.name + '"?')) return;
    this.api.delete(`/productforms/${f.id}`).subscribe({
      next: () => { this.loadForms(); this.loadFormsInUse(); },
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }

  toggleFormActive(f: ProductForm) {
    const body = { ...f, isActive: !f.isActive };
    this.api.put(`/productforms/${f.id}`, body).subscribe({
      next: () => { this.loadForms(); this.loadFormsInUse(); },
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }
}
