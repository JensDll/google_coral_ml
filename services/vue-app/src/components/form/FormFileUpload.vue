<template>
  <div>
    <label>
      <div class="flex justify-between font-semibold mb-2">
        {{ label }}
      </div>
      <div
        class="
          group
          border-2 border-dashed
          py-10
          rounded
          flex
          justify-center
          items-center
          relative
        "
        :class="{ 'bg-red-50 border-red-400': hasError }"
      >
        <input
          class="group w-full h-full absolute opacity-0 cursor-pointer"
          type="file"
          :accept="accept"
          :multiple="multiple"
          @change="handleChange"
        />
        <div class="text-center">
          <p>
            <span
              class="font-semibold text-blue-500 group-hover:text-blue-700"
              :class="{ 'text-red-500 group-hover:text-red-700': hasError }"
            >
              Upload a file
            </span>
            or drag and drop
          </p>
          <slot name="hint"></slot>
          <div
            v-if="image && isFileSelected"
            class="flex flex-col items-center"
          >
            <template v-for="{ file, src } in fileHelpers" :key="src">
              <img :src="src" class="w-32 h-32 mx-auto mb-2 mt-8" />
              <p>{{ file.name }}</p>
            </template>
          </div>
        </div>
      </div>
    </label>
    <div class="text-red-500 text-xs mt-2">
      <p v-for="(error, i) in errors" :key="i">
        {{ error }}
      </p>
    </div>
    <ul class="mt-4">
      <li
        class="flex items-center cursor-pointer group"
        v-for="(file, i) in files"
        :key="file.name"
        @click="removeFile(i)"
      >
        <span class="group-hover:line-through">{{ file.name }}</span>
        <MinusCircleIcon
          class="w-6 h-6 text-red-500 ml-2 group-hover:text-red-700"
        />
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { MinusCircleIcon } from '@heroicons/vue/outline'
import type { PropType } from 'vue'

type FileHelper = {
  src: string
  file: File
}

const props = defineProps({
  label: {
    type: String
  },
  modelValue: {
    type: Array as PropType<File[]>,
    required: true
  },
  errors: {
    type: Array as PropType<string[]>,
    default: () => []
  },
  image: {
    type: Boolean
  },
  multiple: {
    type: Boolean
  },
  accept: {
    type: String
  }
})

const hasError = computed(() => props.errors.length > 0)

const files = computed<File[]>({
  get(): File[] {
    return props.modelValue
  },
  set(files) {
    emit('update:modelValue', files)
  }
})

const fileHelpers = computed<FileHelper[]>(() =>
  files.value.map(file => ({
    src: URL.createObjectURL(file),
    file
  }))
)

const isFileSelected = computed<boolean>(() => fileHelpers.value.length > 0)

const removeFile = (i: number) => {
  URL.revokeObjectURL(fileHelpers.value[i].src)
  files.value.splice(i, 1)
}

const unique = (files: File[]) => {
  const lookup = new Set<string>()
  return files.filter(f => {
    const keep = !lookup.has(f.name)
    lookup.add(f.name)
    return keep
  })
}

const emit = defineEmits(['update:modelValue'])

const handleChange = (e: Event) => {
  const input = e.target as HTMLInputElement
  const fileList = input.files

  if (fileList) {
    for (let i = 0; i < fileList.length; i++) {
      const file = fileList[i]
      files.value[i] = file
    }
    files.value = unique(files.value)
  }

  input.value = ''
}
</script>
