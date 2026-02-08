import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface Distribution {
  id: string;
  name: string;
  contact?: string;
  address?: string;
  phone?: string;
  companyCount?: number;
}

interface DistributionCompany {
  id: string;
  manufacturerId: string;
  manufacturerName: string;
}

@Component({
  selector: 'app-distributions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Distributions</h2>
      <button class="btn btn-primary" (click)="openForm()">Add Distribution</button>
    </div>
    <div class="mb-3">
      <input type="text" class="form-control" placeholder="Search..." [(ngModel)]="search" (input)="load()" style="max-width: 300px;" />
    </div>
    <table class="table table-striped table-hover">
      <thead>
        <tr>
          <th>Name</th>
          <th>Contact</th>
          <th>Phone</th>
          <th>Companies</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (d of distributions; track d.id) {
          <tr>
            <td>{{ d.name }}</td>
            <td>{{ d.contact || '-' }}</td>
            <td>{{ d.phone || '-' }}</td>
            <td>{{ d.companyCount ?? 0 }}</td>
            <td>
              <button type="button" class="btn btn-sm btn-outline-primary me-1" (click)="manageCompanies(d)">Manage Companies</button>
              <button type="button" class="btn btn-sm btn-outline-secondary me-1" (click)="edit(d); $event.stopPropagation()">Edit</button>
              <button class="btn btn-sm btn-outline-danger" (click)="remove(d)">Delete</button>
            </td>
          </tr>
        }
      </tbody>
    </table>

    <!-- Distribution form modal -->
    <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showModal) {
        <div class="modal-backdrop fade show" (click)="closeForm()"></div>
      }
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Edit Distribution' : 'Add Distribution' }}</h5>
            <button type="button" class="btn-close" (click)="closeForm()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-2">
              <label class="form-label">Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" required />
            </div>
            <div class="mb-2">
              <label class="form-label">Contact</label>
              <input type="text" class="form-control" [(ngModel)]="form.contact" />
            </div>
            <div class="mb-2">
              <label class="form-label">Phone</label>
              <input type="text" class="form-control" [(ngModel)]="form.phone" />
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
    </div>

    <!-- Manage companies modal -->
    <div class="modal fade" [class.show]="showCompaniesModal" [style.display]="showCompaniesModal ? 'block' : 'none'" tabindex="-1" role="dialog">
      @if (showCompaniesModal) {
        <div class="modal-backdrop fade show" (click)="closeCompaniesModal()"></div>
      }
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Companies for {{ selectedDistribution?.name }}</h5>
            <button type="button" class="btn-close" (click)="closeCompaniesModal()"></button>
          </div>
          <div class="modal-body">
            <p class="text-muted small">Add manufacturers (companies) that this distribution supplies.</p>
            <div class="mb-3">
              <label class="form-label">Add Manufacturer</label>
              <div class="d-flex gap-2">
                <select class="form-select flex-grow-1" [(ngModel)]="addManufacturerId">
                  <option value="">-- Select manufacturer --</option>
                  @for (m of manufacturers; track m.id) {
                    <option [value]="m.id">{{ m.name }}</option>
                  }
                </select>
                <button class="btn btn-primary" (click)="addCompany()" [disabled]="!addManufacturerId">Add</button>
              </div>
            </div>
            <table class="table table-sm">
              <thead><tr><th>Manufacturer</th><th></th></tr></thead>
              <tbody>
                @for (c of distributionCompanies; track c.id) {
                  <tr>
                    <td>{{ c.manufacturerName }}</td>
                    <td>
                      <button class="btn btn-sm btn-outline-danger" (click)="removeCompany(c)">Remove</button>
                    </td>
                  </tr>
                }
                @if (distributionCompanies.length === 0) {
                  <tr><td colspan="2" class="text-muted">No companies added yet.</td></tr>
                }
              </tbody>
            </table>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeCompaniesModal()">Close</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DistributionsComponent implements OnInit {
  distributions: Distribution[] = [];
  manufacturers: { id: string; name: string }[] = [];
  search = '';
  showModal = false;
  showCompaniesModal = false;
  editingId: string | null = null;
  selectedDistribution: Distribution | null = null;
  distributionCompanies: DistributionCompany[] = [];
  addManufacturerId = '';
  form: Partial<Distribution> = { name: '', contact: '', address: '', phone: '' };

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.load();
    this.api.get<{ id: string; name: string }[]>('/manufacturers').subscribe((m) => (this.manufacturers = m || []));
  }

  load() {
    const params: Record<string, string> = {};
    if (this.search) params['search'] = this.search;
    this.api.get<Distribution[]>('/distributions', params).subscribe((d) => {
      this.distributions = d || [];
      this.cdr.detectChanges();
    });
  }

  openForm() {
    this.editingId = null;
    this.form = { name: '', contact: '', address: '', phone: '' };
    this.showModal = true;
    document.body.classList.add('modal-open');
  }

  edit(d: Distribution) {
    this.editingId = d.id;
    this.form = { id: d.id, name: d.name, contact: d.contact ?? '', address: d.address ?? '', phone: d.phone ?? '' };
    this.showModal = true;
    document.body.classList.add('modal-open');
    this.cdr.detectChanges();
  }

  closeForm() {
    this.showModal = false;
    document.body.classList.remove('modal-open');
  }

  save() {
    if (!this.form.name) return;
    const body = {
      id: this.form.id ?? crypto.randomUUID(),
      name: this.form.name,
      contact: this.form.contact ?? null,
      address: this.form.address ?? null,
      phone: this.form.phone ?? null
    };
    if (this.editingId) {
      this.api.put(`/distributions/${this.editingId}`, body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Update failed')
      });
    } else {
      this.api.post('/distributions', body).subscribe({
        next: () => { this.closeForm(); this.load(); },
        error: (e) => alert(e.error?.message || 'Create failed')
      });
    }
  }

  remove(d: Distribution) {
    if (!confirm('Delete distribution: ' + d.name + '?')) return;
    this.api.delete(`/distributions/${d.id}`).subscribe({
      next: () => this.load(),
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }

  manageCompanies(d: Distribution) {
    this.selectedDistribution = d;
    this.loadDistributionCompanies();
    this.showCompaniesModal = true;
    document.body.classList.add('modal-open');
  }

  closeCompaniesModal() {
    this.showCompaniesModal = false;
    this.selectedDistribution = null;
    this.distributionCompanies = [];
    this.addManufacturerId = '';
    document.body.classList.remove('modal-open');
    this.load();
  }

  loadDistributionCompanies() {
    if (!this.selectedDistribution) return;
    this.api.get<DistributionCompany[]>(`/distributions/${this.selectedDistribution.id}/companies`).subscribe((c) => {
      this.distributionCompanies = c || [];
      this.cdr.detectChanges();
    });
  }

  addCompany() {
    if (!this.selectedDistribution || !this.addManufacturerId) return;
    this.api.post(`/distributions/${this.selectedDistribution.id}/companies`, { manufacturerId: this.addManufacturerId }).subscribe({
      next: () => {
        this.addManufacturerId = '';
        this.loadDistributionCompanies();
      },
      error: (e) => alert(e.error?.message || 'Failed to add company')
    });
  }

  removeCompany(c: DistributionCompany) {
    if (!this.selectedDistribution || !confirm('Remove ' + c.manufacturerName + ' from this distribution?')) return;
    this.api.delete(`/distributions/${this.selectedDistribution.id}/companies/${c.manufacturerId}`).subscribe({
      next: () => this.loadDistributionCompanies(),
      error: (e) => alert(e.error?.message || 'Failed')
    });
  }
}
