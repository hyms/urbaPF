import { defineStore } from 'pinia'
import { api } from '@/boot/api'

export interface Expense {
  id: string
  comunidadId: string
  usuarioId: string
  usuarioName: string
  type: 'INGRESO' | 'EGRESO'
  category: string
  amount: number
  currency: string
  date: string
  description: string
  receiptUrl?: string
  createdAt: string
}

export interface ExpenseSummary {
  balance: number
  totalMonthlyExpenses: number
  topExpenses: Expense[]
}

interface ExpenseState {
  expenses: Expense[]
  summary: ExpenseSummary | null
  loading: boolean
  error: string | null
}

export const useExpenseStore = defineStore('expense', {
  state: (): ExpenseState => ({
    expenses: [],
    summary: null,
    loading: false,
    error: null
  }),

  actions: {
    async fetchExpenses() {
      this.loading = true
      try {
        const response = await api.get('/expenses')
        this.expenses = response.data
      } catch (error) {
        this.error = (error as Error).message
      } finally {
        this.loading = false
      }
    },

    async fetchSummary() {
      this.loading = true
      try {
        const response = await api.get('/expenses/summary')
        this.summary = response.data
      } catch (error) {
        this.error = (error as Error).message
      } finally {
        this.loading = false
      }
    },

    async createExpense(data: Partial<Expense>) {
      this.loading = true
      try {
        await api.post('/expenses', data)
        await this.fetchExpenses()
        await this.fetchSummary()
        return true
      } catch (error) {
        this.error = (error as Error).message
        return false
      } finally {
        this.loading = false
      }
    },

    async updateExpense(id: string, data: Partial<Expense>) {
      this.loading = true
      try {
        await api.put(`/expenses/${id}`, data)
        await this.fetchExpenses()
        await this.fetchSummary()
        return true
      } catch (error) {
        this.error = (error as Error).message
        return false
      } finally {
        this.loading = false
      }
    },

    async deleteExpense(id: string) {
      this.loading = true
      try {
        await api.delete(`/expenses/${id}`)
        await this.fetchExpenses()
        await this.fetchSummary()
        return true
      } catch (error) {
        this.error = (error as Error).message
        return false
      } finally {
        this.loading = false
      }
    }
  }
})