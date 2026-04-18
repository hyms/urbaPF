<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h4">{{ t('expenses.title') }}</div>
      <q-space />
      <q-btn 
        v-if="authStore.isManager || authStore.isAdmin"
        color="primary" 
        icon="add" 
        :label="t('expenses.newExpense')" 
        @click="openExpenseDialog()" 
      />
    </div>

    <!-- Summary Widgets (Visible to all) -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-md-4">
        <q-card flat bordered class="bg-primary text-white rounded-lg">
          <q-card-section>
            <div class="text-subtitle2">{{ t('expenses.balance') }}</div>
            <div class="text-h3 text-weight-bold">
              {{ formatCurrency(expenseStore.summary?.balance || 0) }}
            </div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-4">
        <q-card flat bordered class="rounded-lg">
          <q-card-section>
            <div class="text-subtitle2 text-grey-7">{{ t('expenses.monthlyTotal') }}</div>
            <div class="text-h3 text-weight-bold text-negative">
              {{ formatCurrency(expenseStore.summary?.totalMonthlyExpenses || 0) }}
            </div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-4">
        <q-card flat bordered class="rounded-lg">
          <q-card-section>
            <div class="text-subtitle2 text-grey-7">{{ t('expenses.topExpenses') }}</div>
            <q-list dense>
              <q-item v-for="exp in expenseStore.summary?.topExpenses" :key="exp.id" class="q-px-none">
                <q-item-section>
                  <q-item-label class="text-weight-medium">{{ exp.category }}</q-item-label>
                  <q-item-label caption>{{ exp.description }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <div class="text-weight-bold text-negative">{{ formatCurrency(exp.amount) }}</div>
                </q-item-section>
              </q-item>
              <div v-if="!expenseStore.summary?.topExpenses?.length" class="text-caption text-grey-5 q-pa-sm">
                Sin gastos este mes
              </div>
            </q-list>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Full Table (Visible to all, but only Managers can Edit/Delete) -->
    <q-card flat bordered class="rounded-lg">
      <q-table
        :rows="expenseStore.expenses"
        :columns="columns"
        row-key="id"
        :loading="expenseStore.loading"
        flat
        :pagination="{ rowsPerPage: 10 }"
      >
        <template v-slot:body-cell-type="props">
          <q-td :props="props">
            <q-chip 
              :color="props.row.type === 'INGRESO' ? 'positive' : 'negative'" 
              text-color="white" 
              size="sm"
            >
              {{ props.row.type === 'INGRESO' ? t('expenses.income') : t('expenses.expense') }}
            </q-chip>
          </q-td>
        </template>

        <template v-slot:body-cell-amount="props">
          <q-td :props="props" :class="props.row.type === 'INGRESO' ? 'text-positive' : 'text-negative'">
            {{ props.row.type === 'INGRESO' ? '+' : '-' }} {{ formatCurrency(props.row.amount) }}
          </q-td>
        </template>

        <template v-slot:body-cell-receipt="props">
          <q-td :props="props">
            <q-btn 
              v-if="props.row.receiptUrl" 
              flat round dense 
              icon="attach_file" 
              color="primary"
              @click="viewReceipt(props.row.receiptUrl)"
            >
              <q-tooltip>{{ t('expenses.receipt') }}</q-tooltip>
            </q-btn>
            <span v-else class="text-grey-4">-</span>
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props">
            <template v-if="authStore.isManager || authStore.isAdmin">
              <q-btn flat round dense icon="edit" @click="editExpense(props.row)" />
              <q-btn flat round dense icon="delete" color="negative" @click="confirmDelete(props.row)" />
            </template>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width column items-center q-pa-lg">
            <q-icon name="payments" size="64px" color="grey-4" />
            <div class="text-h6 text-grey-6 q-mt-md">{{ t('expenses.noExpenses') }}</div>
          </div>
        </template>
      </q-table>
    </q-card>

    <!-- Create/Edit Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card :style="$q.screen.lt.sm ? 'width: 100%; max-width: 100%;' : 'min-width: 400px'" class="rounded-lg">
        <q-card-section class="row items-center">
          <div class="text-h6">{{ editingExpense ? t('expenses.editExpense') : t('expenses.newExpense') }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-form @submit="saveExpense">
          <q-card-section class="q-gutter-md">
            <q-btn-toggle
              v-model="form.type"
              spread
              no-caps
              rounded
              unelevated
              toggle-color="primary"
              color="white"
              text-color="primary"
              :options="[
                { label: t('expenses.income'), value: 'INGRESO' },
                { label: t('expenses.expense'), value: 'EGRESO' }
              ]"
            />
            
            <q-input 
              v-model.number="form.amount" 
              type="number" 
              :label="t('expenses.amount')" 
              prefix="Bs."
              required 
              outlined 
              dense 
            />

            <q-input 
              v-model="form.date" 
              type="date" 
              :label="t('expenses.date')" 
              required 
              outlined 
              dense 
            />

            <q-input 
              v-model="form.description" 
              type="textarea" 
              :label="t('expenses.description')" 
              outlined 
              dense 
              rows="3"
            />

            <q-input 
              v-model="form.receiptUrl" 
              :label="t('expenses.receipt') + ' (URL)'" 
              outlined 
              dense 
            >
              <template v-slot:append>
                <q-icon name="link" />
              </template>
            </q-input>
          </q-card-section>

          <q-card-actions align="right" class="q-pb-md q-pr-md">
            <q-btn flat :label="t('common.cancel')" v-close-popup />
            <q-btn 
              v-if="authStore.isManager || authStore.isAdmin"
              color="primary" 
              type="submit" 
              :label="t('common.save')" 
              :loading="expenseStore.loading" 
            />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useQuasar } from 'quasar'
import { useI18n } from '../composables/useI18n'
import { useAuthStore } from '../stores/auth'
import { useExpenseStore, Expense } from '../stores/expense'
import { formatCurrency, formatDate } from '@/utils/format'

const $q = useQuasar()
const { t } = useI18n()
const authStore = useAuthStore()
const expenseStore = useExpenseStore()

const showDialog = ref(false)
const editingExpense = ref<Expense | null>(null)

const form = reactive({
  type: 'EGRESO' as 'INGRESO' | 'EGRESO',
  category: '',
  amount: 0,
  date: new Date().toISOString().split('T')[0],
  description: '',
  receiptUrl: '',
  currency: 'BOB'
})

const columns = computed(() => [
  { name: 'date', label: t('expenses.date'), field: 'date', format: (val: string) => formatDate(val), sortable: true, align: 'left' as const },
  { name: 'type', label: t('expenses.type'), field: 'type', align: 'center' as const },
  { name: 'amount', label: t('expenses.amount'), field: 'amount', sortable: true, align: 'right' as const },
  { name: 'responsible', label: t('expenses.responsible'), field: 'usuarioName', align: 'left' as const },
  { name: 'receipt', label: '', field: 'receiptUrl', align: 'center' as const },
  { name: 'actions', label: '', field: 'id', align: 'center' as const }
])

function openExpenseDialog(expense: Expense | null = null) {
  if (expense) {
    editingExpense.value = expense
    Object.assign(form, {
      ...expense,
      date: expense.date.split('T')[0]
    })
  } else {
    editingExpense.value = null
    Object.assign(form, {
      type: 'EGRESO',
      category: '',
      amount: 0,
      date: new Date().toISOString().split('T')[0],
      description: '',
      receiptUrl: '',
      currency: 'BOB'
    })
  }
  showDialog.value = true
}

function editExpense(expense: Expense) {
  openExpenseDialog(expense)
}

async function saveExpense() {
  let success = false
  if (editingExpense.value) {
    success = await expenseStore.updateExpense(editingExpense.value.id, form)
  } else {
    success = await expenseStore.createExpense(form)
  }

  if (success) {
    $q.notify({ type: 'positive', message: t('common.success') })
    showDialog.value = false
  } else {
    $q.notify({ type: 'negative', message: expenseStore.error || t('common.error') })
  }
}

function confirmDelete(expense: Expense) {
  $q.dialog({
    title: t('common.confirmDelete'),
    message: t('expenses.confirmDelete'),
    cancel: true,
    persistent: true
  }).onOk(async () => {
    const success = await expenseStore.deleteExpense(expense.id)
    if (success) {
      $q.notify({ type: 'positive', message: t('common.success') })
    } else {
      $q.notify({ type: 'negative', message: expenseStore.error || t('common.error') })
    }
  })
}

function viewReceipt(url: string) {
  window.open(url, '_blank')
}

function viewDetails(expense: Expense) {
  // TODO: Implement read-only details view for neighbors if needed
}

onMounted(async () => {
  await Promise.all([
    expenseStore.fetchExpenses(),
    expenseStore.fetchSummary()
  ])
})
</script>