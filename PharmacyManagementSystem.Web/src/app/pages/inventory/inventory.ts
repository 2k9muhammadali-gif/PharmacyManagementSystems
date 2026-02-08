import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Inventory</h2>
    @if (branches.length > 0 && !selectedBranch) {
      <p>Select a branch: </p>
      <div class="mb-3">
        @for (b of branches; track b.id) {
          <button class="btn btn-outline-primary me-2" (click)="loadInventory(b.id)">{{ b.name }}</button>
        }
      </div>
    }
    @if (selectedBranch) {
      <p>Branch: {{ selectedBranch }}</p>
      <table class="table table-striped">
        <thead>
          <tr><th>Product</th><th>Batch</th><th>Qty</th><th>Expiry</th></tr>
        </thead>
        <tbody>
          @for (i of inventory; track i.id) {
            <tr>
              <td>{{ i.productName }}</td>
              <td>{{ i.batchNumber }}</td>
              <td>{{ i.quantity }}</td>
              <td>{{ i.expiryDate | date }}</td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class InventoryComponent implements OnInit {
  branches: { id: string; name: string }[] = [];
  inventory: { id: string; productName: string; batchNumber: string; quantity: number; expiryDate: string }[] = [];
  selectedBranch: string | null = null;

  constructor(private api: ApiService, private auth: AuthService) {}

  ngOnInit() {
    this.api.get<{ id: string; name: string }[]>('/branches').subscribe((b) => {
      this.branches = b || [];
      const branchId = this.auth.user()?.branchId;
      if (branchId && this.branches.length) this.loadInventory(branchId);
    });
  }

  loadInventory(branchId: string) {
    this.selectedBranch = this.branches.find((b) => b.id === branchId)?.name ?? branchId;
    this.api.get<{ id: string; productName: string; batchNumber: string; quantity: number; expiryDate: string }[]>('/inventory', { branchId }).subscribe((i) => (this.inventory = i || []));
  }
}
