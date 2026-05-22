export type Ok<T> = { readonly ok: true; readonly data: T }
export type Err<E> = { readonly ok: false; readonly error: E }
export type Result<T, E = string> = Ok<T> | Err<E>

export const ok = <T>(data: T): Ok<T> => ({ ok: true, data })
export const err = <E>(error: E): Err<E> => ({ ok: false, error })

export function isOk<T, E>(result: Result<T, E>): result is Ok<T> {
  return result.ok === true
}
