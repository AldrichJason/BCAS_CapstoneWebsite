export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
}

export interface LoginResponseDto {
  token: string;
  expiresAtUtc: string;
  user: UserDto;
}

export interface ErrorResponseDto {
  message: string;
}
