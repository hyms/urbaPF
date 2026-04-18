export interface User {
  id: string
  email: string
  fullName: string
  phone?: string
  photoUrl?: string
  role?: number
  forcePasswordChange?: boolean
}

export interface UpdateUserRequest {
  fullName?: string;
  phone?: string;
  photoUrl?: string;
  role?: number;
  credibilityLevel?: number;
  status?: number;
  condominiumId?: string;
  lotNumber?: string;
  streetAddress?: string;
  isValidated?: boolean;
  managerVotes?: number;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}
