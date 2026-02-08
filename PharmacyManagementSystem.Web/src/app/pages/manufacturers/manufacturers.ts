import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface Manufacturer {
  id: string;
  name: string;
  contactInfo?: string;
  address?: string;
}

@Component({
  selector: 'app-manufacturers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Manufacturers</h2>
      <button class="btn btn-primary" (click)="openForm()">Add Manufacturer</button>
    </div>
    <div class="mb-3">
      <input type="text" class="form-control" placeholder="Search..." [(ngModel)]="search" (input)="load()" style="max-width: 300px;" />
    </div>
    <table class="table table-striped table-hover">
      <thead>
        <tr><th>Name</th><th>Contact</th><th>Address</th><th></th></tr>
      </thead>
      <tbody>
        @for (m of manufacturers; track m.id) {
          <tr>
            <td>{{ m.name }}</td>
            <td>{{ m.contactInfo || '-' }}</td>
            <td>{{ m.address || '-' }}</td>
            <td>
              <button class="btn btn-sm btn-outline-primary me-1" (click)="edit(m)">Edit</button>
              <button class="btn btn-sm btn-outline-danger" (click)="remove(m)">Delete</button>
            </td>
          </tr>
        }
      </tbody>
    </table>

    <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Edit Manufacturer' : 'Add Manufacturer' }}</h5>
            <button type="button" class="btn-close" (click)="closeForm()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" required />
            </div>
            <div class="mb-2">
              <label class="form-label">Contact</label>
              <input type="text" class="form-control" [(ngModel)]="form.contactInfo" />
            </div>
            <div class="mb-2">
              <label class="form-label">Address</label>
              <textarea class="form-control" [(ngModel)]="form.address" rows="2"></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeForm()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="save()">Save</button>
          </div>
        </div>
      </div>
      @if (showModal) {
        <div class="modal-backdrop fade show"></div>
      }
    </div>
  `
})
export class ManufacturersComponent implements OnInit {
  manufacturers: Manufacturer[] = [];
  search = '';
  showModal = false;
  editingId: string | null = null;
  form: Partial<Manufacturer> = { name: '', contactInfo: '', address: '' };

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    const params: Record<string, string> = {};
    if (this.search) params['search'] = this.search;
    this.api.get<Manufacturer[]>('/manufacturers', params).subscribe((m) => (this.manufacturers = m || []));
  }

  openForm() {
    this.editingId = null;
    this.form = { name: '', contactInfo: '', address: '' };
    this.showModal = true;
  }

  edit(m: Manufacturer) {
    this.editingId = m.id;
    this.form = { id: m.id, name: m.name, contactInfo: m.contactInfo ?? '', address: m.address ?? '' };
    this.showModal = true;
  }

  closeForm() {
    this.showModal = false;
  }

  save() {
    if (!this.form.name) return;
    const body = {
      id: this.form.id ?? crypto.randomUUID(),
      name: this.form.name,
      contactInfo: this.form.contactInfo ?? null,
      address: this.form.address ?? null
    };
    if (this.editingId) {
      this.api.put(`/manufacturers/${this.editingId}`, body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/manufacturers', body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }

  remove(m: Manufacturer) {
    if (!confirm('Delete manufacturer: ' + m.name + '?')) return;
    this.api.delete(`/manufacturers/${m.id}`).subscribe({
      next: () => this.load(),
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }
}
