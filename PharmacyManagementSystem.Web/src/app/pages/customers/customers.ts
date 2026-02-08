import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface Customer {
  id: string;
  name: string;
  phone?: string;
  cnic?: string;
  email?: string;
  creditLimit?: number;
  address?: string;
}

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Customers</h2>
      <button class="btn btn-primary" (click)="openForm()">Add Customer</button>
    </div>
    <div class="mb-3">
      <input type="text" class="form-control" placeholder="Search..." [(ngModel)]="search" (input)="load()" style="max-width: 300px;" />
    </div>
    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Name</th>
          <th>Phone</th>
          <th>CNIC</th>
          <th>Email</th>
          <th>Credit Limit</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (c of customers; track c.id) {
          <tr>
            <td>{{ c.name }}</td>
            <td>{{ c.phone || '-' }}</td>
            <td>{{ c.cnic || '-' }}</td>
            <td>{{ c.email || '-' }}</td>
            <td>Rs {{ (c.creditLimit ?? 0) | number:'1.2-2' }}</td>
            <td>
              <button class="btn btn-sm btn-outline-primary" (click)="edit(c)">Edit</button>
            </td>
          </tr>
        }
      </tbody>
    </table>

    <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Edit Customer' : 'Add Customer' }}</h5>
            <button type="button" class="btn-close" (click)="closeForm()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" required />
            </div>
            <div class="row">
              <div class="col-md-6 mb-2">
                <label class="form-label">Phone</label>
                <input type="text" class="form-control" [(ngModel)]="form.phone" />
              </div>
              <div class="col-md-6 mb-2">
                <label class="form-label">CNIC</label>
                <input type="text" class="form-control" [(ngModel)]="form.cnic" />
              </div>
            </div>
            <div class="mb-2">
              <label class="form-label">Email</label>
              <input type="email" class="form-control" [(ngModel)]="form.email" />
            </div>
            <div class="mb-2">
              <label class="form-label">Address</label>
              <textarea class="form-control" [(ngModel)]="form.address" rows="2"></textarea>
            </div>
            <div class="mb-2">
              <label class="form-label">Credit Limit (Rs)</label>
              <input type="number" step="0.01" class="form-control" [(ngModel)]="form.creditLimit" />
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
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  search = '';
  showModal = false;
  editingId: string | null = null;
  form: Partial<Customer> = { name: '', phone: '', cnic: '', email: '', address: '', creditLimit: 0 };

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    const params: Record<string, string> = {};
    if (this.search) params['search'] = this.search;
    this.api.get<Customer[]>('/customers', params).subscribe((c) => (this.customers = c || []));
  }

  openForm() {
    this.editingId = null;
    this.form = { name: '', phone: '', cnic: '', email: '', address: '', creditLimit: 0 };
    this.showModal = true;
  }

  edit(c: Customer) {
    this.editingId = c.id;
    this.api.get<Customer>(`/customers/${c.id}`).subscribe((cu) => {
      this.form = { ...cu };
      this.showModal = true;
    });
  }

  closeForm() {
    this.showModal = false;
  }

  save() {
    if (!this.form.name) return;
    const body = {
      id: this.form.id ?? crypto.randomUUID(),
      name: this.form.name,
      phone: this.form.phone ?? null,
      cnic: this.form.cnic ?? null,
      email: this.form.email ?? null,
      address: this.form.address ?? null,
      creditLimit: this.form.creditLimit ?? 0
    };
    if (this.editingId) {
      this.api.put(`/customers/${this.editingId}`, body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/customers', body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }
}
