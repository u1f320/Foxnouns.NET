export type PaginatedArray<T> = {
	data: T[];
	currentPage: number;
	pageCount: number;
};

/**
 * Paginates an array.
 * @param arr The array to paginate.
 * @param page The zero-indexed page number.
 * @param perPage How many items to display per page.
 * @returns An object containing a slice of the array, the current page number, and the page count.
 */
export default function paginate<T>(
	arr: T[] | null,
	page: string | number | null,
	perPage: number,
): PaginatedArray<T> {
	if (arr && arr.length > 0) {
		let currentPage = 0;
		if (page && typeof page === "string") currentPage = parseInt(page);
		if (page && typeof page === "number") currentPage = page;

		const pageCount = Math.ceil(arr.length / perPage);
		let data = arr.slice(currentPage * perPage, (currentPage + 1) * perPage);
		if (data.length === 0) {
			data = arr.slice(0, perPage);
			currentPage = 0;
		}

		return { data, currentPage, pageCount };
	}

	return { data: [], currentPage: 0, pageCount: 1 };
}
