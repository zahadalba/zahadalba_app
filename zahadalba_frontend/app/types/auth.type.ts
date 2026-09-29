export interface SignInRequestDto {
  email: string;
  password: string;
}

export interface SignInResponseDto {
  id: string;
  email: string;
}

export interface GetMeResponseDto {
  id: string;
  email: string;
}
