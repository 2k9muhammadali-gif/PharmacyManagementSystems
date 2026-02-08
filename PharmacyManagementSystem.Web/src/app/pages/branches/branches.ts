import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface Branch {
  id: string;
  name: string;
  address: string;
  phone?: string;
  isActive: boolean;
  fbrStoreId?: string;
}

@Component({
  selector: 'app-branches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Branches</h2>
      <button class="btn btn-primary" (click)="openForm()">Add Branch</button>
    </div>
    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Name</th>
          <th>Address</th>
          <th>Phone</th>
          <th>Status</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (b of branches; track b.id) {
          <tr>
            <td>{{ b.name }}</td>
            <td>{{ b.address }}</td>
            <td>{{ b.phone || '-' }}</td>
            <td><span class="badge" [class.bg-success]="b.isActive" [class.bg-secondary]="!b.isActive">{{ b.isActive ? 'Active' : 'Inactive' }}</span></td>
            <td>
              <button type="button" class="btn btn-sm btn-outline-primary" (click)="edit(b); $event.stopPropagation()">Edit</button>
            </td>
          </tr>
        }
      </tbody>
    </table>

    <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1" role="dialog" [attr.aria-hidden]="!showModal">
      @if (showModal) {
        <div class="modal-backdrop fade show" (click)="closeForm()"></div>
      }
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Edit Branch' : 'Add Branch' }}</h5>
            <button type="button" class="btn-close" (click)="closeForm()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" required />
            </div>
            <div class="mb-2">
              <label class="form-label">Address *</label>
              <textarea class="form-control" [(ngModel)]="form.address" rows="2" required></textarea>
            </div>
            <div class="mb-2">
              <label class="form-label">Phone</label>
              <input type="text" class="form-control" [(ngModel)]="form.phone" />
            </div>
            <div class="mb-2">
              <label class="form-label">FBR Store ID</label>
              <input type="text" class="form-control" [(ngModel)]="form.fbrStoreId" />
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
export class BranchesComponent implements OnInit {
  branches: Branch[] = [];
  showModal = false;
  editingId: string | null = null;
  form: Partial<Branch> = { name: '', address: '', phone: '', fbrStoreId: '', isActive: true };

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.get<Branch[]>('/branches').subscribe((b) => {
      this.branches = b || [];
      this.cdr.detectChanges();
    });
  }

  openForm() {
    this.editingId = null;
    this.form = { name: '', address: '', phone: '', fbrStoreId: '', isActive: true };
    this.showModal = true;
    document.body.classList.add('modal-open');
  }

  edit(b: Branch) {
    this.editingId = b.id;
    this.form = { ...b };
    this.showModal = true;
    document.body.classList.add('modal-open');
    this.cdr.detectChanges();
  }

  closeForm() {
    this.showModal = false;
    document.body.classList.remove('modal-open');
  }

  save() {
    if (!this.form.name || !this.form.address) return;
    if (this.editingId) {
      this.api.put(`/branches/${this.editingId}`, {
        name: this.form.name,
        address: this.form.address,
        phone: this.form.phone ?? null,
        fbrStoreId: this.form.fbrStoreId ?? null,
        isActive: this.form.isActive ?? true
      }).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/branches', {
        name: this.form.name,
        address: this.form.address,
        phone: this.form.phone ?? null,
        fbrStoreId: this.form.fbrStoreId ?? null
      }).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }
}
