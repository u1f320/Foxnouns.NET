<script lang="ts">
	import { Pagination, PaginationItem, PaginationLink } from "@sveltestrap/sveltestrap";

	type Props = {
		currentPage: number;
		pageCount: number;
		center?: boolean;
	};
	let { currentPage = $bindable(), pageCount, center }: Props = $props();

	let prevPage = $derived(currentPage > 0 ? currentPage - 1 : 0);
	let nextPage = $derived(currentPage < pageCount - 1 ? currentPage + 1 : pageCount - 1);
</script>

{#if pageCount > 1}
	<div>
		<Pagination listClassName={center ? "justify-content-center" : undefined}>
			<PaginationItem>
				<PaginationLink first onclick={() => (currentPage = 0)} />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink previous onclick={() => (currentPage = prevPage)} />
			</PaginationItem>
			{#each new Array(pageCount) as _, page}
				<PaginationItem active={page === currentPage}>
					<PaginationLink onclick={() => (currentPage = page)}>{page + 1}</PaginationLink>
				</PaginationItem>
			{/each}
			<PaginationItem>
				<PaginationLink next onclick={() => (currentPage = nextPage)} />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink last onclick={() => (currentPage = pageCount - 1)} />
			</PaginationItem>
		</Pagination>
	</div>
{/if}
