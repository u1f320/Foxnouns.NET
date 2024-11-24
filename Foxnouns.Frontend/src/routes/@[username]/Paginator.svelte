<script lang="ts">
	import { Pagination, PaginationItem, PaginationLink } from "@sveltestrap/sveltestrap";

	type Props = { currentPage: number; pageCount: number; href: string };
	let { currentPage, pageCount, href }: Props = $props();

	let prevPage = $derived(currentPage > 0 ? currentPage - 1 : 0);
	let prevLink = $derived(prevPage !== 0 ? `${href}?page=${prevPage}` : href);

	let nextPage = $derived(currentPage < pageCount - 1 ? currentPage + 1 : pageCount - 1);
</script>

{#if pageCount > 1}
	<Pagination>
		<PaginationItem>
			<PaginationLink first {href} />
		</PaginationItem>
		<PaginationItem>
			<PaginationLink previous href={prevLink} />
		</PaginationItem>
		<PaginationItem active>
			<PaginationLink href="{href}?page={currentPage}">{currentPage + 1}</PaginationLink>
		</PaginationItem>
		<PaginationItem>
			<PaginationLink next href="{href}?page={nextPage}" />
		</PaginationItem>
		<PaginationItem>
			<PaginationLink last href="{href}?page={pageCount - 1}" />
		</PaginationItem>
	</Pagination>
{/if}
