package com.retailops.mobile.profile.domain

import com.retailops.mobile.profile.domain.repository.IAuthRepository
import io.mockk.coEvery
import io.mockk.mockk
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class GetCurrentUserUseCaseTest {
    @Test
    fun returnsSuccessFromRepository() = runTest {
        val user = User(
            id = "id",
            legacyUserId = 1,
            tenantId = 10,
            name = "Nome",
            email = "a@b.com",
            userLevel = "Vendedor",
            permissionKeys = listOf("vendas"),
        )
        val repo = mockk<IAuthRepository>()
        coEvery { repo.getCurrentUser() } returns Result.Success(user)

        val result = GetCurrentUserUseCase(repo)()
        assertTrue(result is Result.Success)
        assertEquals(user, (result as Result.Success).value)
    }

    @Test
    fun propagatesFailure() = runTest {
        val repo = mockk<IAuthRepository>()
        coEvery { repo.getCurrentUser() } returns Result.Failure(Result.UNAUTHORIZED)

        val result = GetCurrentUserUseCase(repo)()
        assertTrue(result is Result.Failure)
        assertEquals(Result.UNAUTHORIZED, (result as Result.Failure).message)
    }
}
