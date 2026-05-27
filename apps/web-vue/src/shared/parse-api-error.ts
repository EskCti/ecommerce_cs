type ProblemDetails = {
  error?: string
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export async function parseApiError(res: Response, fallback: string): Promise<string> {
  try {
    const body = (await res.json()) as ProblemDetails

    if (body.error?.trim()) return body.error

    if (body.errors) {
      const messages = Object.values(body.errors)
        .flat()
        .filter((message) => message?.trim())
      if (messages.length > 0) return messages.join(' • ')
    }

    if (body.detail?.trim()) return body.detail
    if (body.title?.trim() && body.title !== 'One or more validation errors occurred.') {
      return body.title
    }

    return fallback
  } catch {
    return fallback
  }
}
