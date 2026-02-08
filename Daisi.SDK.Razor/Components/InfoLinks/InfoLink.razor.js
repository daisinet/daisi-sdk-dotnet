export function loadPopovers() {
	//if (!window.popoverList) {
		window.popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
		window.popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));
	//}
}