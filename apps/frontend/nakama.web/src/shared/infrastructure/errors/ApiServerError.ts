export class ApiServerError extends Error {
  readonly status: number | null;

  constructor(message: string, status: number | null) {
    super(message);
    this.name = "ApiServerError";
    this.status = status;
  }
}
