// src/app/components/leave-request/leave-request.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LeaveService } from '../../services/leave.service';

@Component({
  selector: 'app-leave-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-gray-50 p-8">
      <div class="max-w-lg mx-auto bg-white rounded-2xl shadow-sm p-8">
        <h2 class="text-2xl font-bold text-gray-800 mb-6">Apply for Leave</h2>

        <form (ngSubmit)="submit()" #form="ngForm">
          <div class="mb-5">
            <label class="block text-sm font-medium text-gray-700 mb-1">Leave Type</label>
            <select [(ngModel)]="request.leaveTypeID" name="leaveTypeID" required
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="">Select leave type</option>
              <option [value]="1">Annual Leave (21 days)</option>
              <option [value]="2">Sick Leave (14 days)</option>
              <option [value]="3">Maternity Leave (90 days)</option>
              <option [value]="4">Paternity Leave (14 days)</option>
              <option [value]="5">Unpaid Leave (30 days)</option>
              <option [value]="6">Emergency Leave (3 days)</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4 mb-5">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
              <input type="date" [(ngModel)]="request.startDate" name="startDate" required
                [min]="today"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
              <input type="date" [(ngModel)]="request.endDate" name="endDate" required
                [min]="request.startDate || today"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
            </div>
          </div>

          <div *ngIf="totalDays > 0" class="mb-5 p-3 bg-blue-50 rounded-lg text-blue-700 text-sm font-medium">
            Total Days: {{ totalDays }} working day(s)
          </div>

          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 mb-1">Reason</label>
            <textarea [(ngModel)]="request.reason" name="reason" required rows="4"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Please provide a reason for your leave request"></textarea>
          </div>

          <div *ngIf="errorMessage" class="mb-4 p-3 bg-red-50 text-red-600 rounded-lg text-sm">
            {{ errorMessage }}
          </div>
          <div *ngIf="successMessage" class="mb-4 p-3 bg-green-50 text-green-600 rounded-lg text-sm">
            {{ successMessage }}
          </div>

          <div class="flex gap-3">
            <button type="submit" [disabled]="loading"
              class="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-lg transition disabled:opacity-50">
              {{ loading ? 'Submitting...' : 'Submit Request' }}
            </button>
            <button type="button" (click)="goBack()"
              class="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold py-3 rounded-lg transition">
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class LeaveRequestComponent implements OnInit {
  request = { leaveTypeID: 0, startDate: '', endDate: '', reason: '' };
  today         = new Date().toISOString().split('T')[0];
  loading       = false;
  errorMessage  = '';
  successMessage = '';
  totalDays     = 0;

  constructor(private leaveService: LeaveService, private router: Router) {}

  ngOnInit(): void {}

  get calculatedDays(): number {
    return this.leaveService.calculateDays(this.request.startDate, this.request.endDate);
  }

  submit(): void {
    this.totalDays = this.calculatedDays;
    this.loading   = true;
    this.errorMessage = '';

    this.leaveService.applyLeave({
      leaveTypeID: +this.request.leaveTypeID,
      startDate:   this.request.startDate,
      endDate:     this.request.endDate,
      reason:      this.request.reason
    }).subscribe({
      next: res => {
        this.successMessage = res.message;
        this.loading = false;
        setTimeout(() => this.router.navigate(['/my-requests']), 1500);
      },
      error: err => {
        this.errorMessage = err.error?.message ?? 'Failed to submit request';
        this.loading = false;
      }
    });
  }

  goBack(): void { this.router.navigate(['/dashboard']); }
}
