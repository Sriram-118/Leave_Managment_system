// src/app/components/leave-list/leave-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaveService } from '../../services/leave.service';
import { LeaveRequestResponse, ReviewRequest } from '../../models/models';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-leave-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-gray-50 p-8">
      <div class="max-w-5xl mx-auto">
        <h2 class="text-2xl font-bold text-gray-800 mb-6">
          {{ isManagerView ? 'Pending Reviews' : 'My Leave Requests' }}
        </h2>

        <div *ngIf="requests.length === 0" class="bg-white rounded-xl p-8 text-center text-gray-400">
          No leave requests found
        </div>

        <div *ngFor="let req of requests" class="bg-white rounded-xl shadow-sm p-6 mb-4">
          <div class="flex justify-between items-start">
            <div>
              <p class="font-semibold text-gray-800">{{ req.leaveType }}</p>
              <p *ngIf="isManagerView" class="text-sm text-gray-500">{{ req.employeeName }}</p>
              <p class="text-sm text-gray-500 mt-1">
                {{ req.startDate | date:'dd MMM yyyy' }} – {{ req.endDate | date:'dd MMM yyyy' }}
                ({{ req.totalDays }} days)
              </p>
              <p class="text-sm text-gray-600 mt-2">{{ req.reason }}</p>
              <p *ngIf="req.reviewNote" class="text-sm text-orange-600 mt-1">Note: {{ req.reviewNote }}</p>
            </div>
            <div class="text-right">
              <span class="px-3 py-1 rounded-full text-xs font-medium"
                [ngClass]="{
                  'bg-yellow-100 text-yellow-700': req.status === 'Pending',
                  'bg-green-100 text-green-700':  req.status === 'Approved',
                  'bg-red-100 text-red-700':      req.status === 'Rejected',
                  'bg-gray-100 text-gray-600':    req.status === 'Cancelled'
                }">{{ req.status }}</span>

              <div class="mt-3 flex gap-2 justify-end">
                <!-- Employee cancel -->
                <button *ngIf="!isManagerView && req.status === 'Pending'"
                  (click)="cancel(req.requestID)"
                  class="text-xs text-red-500 hover:text-red-700 border border-red-300 px-3 py-1 rounded-lg transition">
                  Cancel
                </button>

                <!-- Manager review -->
                <ng-container *ngIf="isManagerView && req.status === 'Pending'">
                  <input [(ngModel)]="reviewNotes[req.requestID]" placeholder="Optional note"
                    class="text-xs border border-gray-200 px-2 py-1 rounded-lg w-40">
                  <button (click)="review(req.requestID, 'Approved')"
                    class="text-xs bg-green-500 text-white px-3 py-1 rounded-lg hover:bg-green-600">Approve</button>
                  <button (click)="review(req.requestID, 'Rejected')"
                    class="text-xs bg-red-500 text-white px-3 py-1 rounded-lg hover:bg-red-600">Reject</button>
                </ng-container>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LeaveListComponent implements OnInit {
  requests: LeaveRequestResponse[] = [];
  reviewNotes: { [key: number]: string } = {};
  isManagerView = false;

  constructor(private leaveService: LeaveService, public auth: AuthService) {}

  ngOnInit(): void {
    const path = window.location.pathname;
    this.isManagerView = path.includes('pending-reviews');

    if (this.isManagerView) {
      this.leaveService.getPendingReviews().subscribe(r => this.requests = r);
    } else {
      this.leaveService.getMyRequests().subscribe(r => this.requests = r);
    }
  }

  cancel(id: number): void {
    if (!confirm('Cancel this leave request?')) return;
    this.leaveService.cancelLeave(id).subscribe(() =>
      this.requests = this.requests.filter(r => r.requestID !== id)
    );
  }

  review(id: number, status: string): void {
    const dto: ReviewRequest = { status, reviewNote: this.reviewNotes[id] };
    this.leaveService.reviewLeave(id, dto).subscribe(() =>
      this.requests = this.requests.filter(r => r.requestID !== id)
    );
  }
}
