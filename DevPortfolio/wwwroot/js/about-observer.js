export function observeToggle(sectionId) {
    const section = document.getElementById(sectionId);
    if (!section) return;
    const inner = section.querySelector(".about-inner");
    if (!inner) return;

    const obs = new IntersectionObserver(
        entries => entries.forEach(e => {
            if (e.isIntersecting) inner.classList.add("visible");
            else inner.classList.remove("visible");
        }),
        { threshold: 0.35 }
    );
    obs.observe(section);
}

export function observeMany(selector) {
    const els = Array.from(document.querySelectorAll(selector));
    if (!els.length) return;

    const markIfInView = (el) => {
        const r = el.getBoundingClientRect();
        const vh = window.innerHeight || document.documentElement.clientHeight;
        if (r.top < vh * 0.9 && r.bottom > 0) el.classList.add("visible");
    };

    const obs = new IntersectionObserver(
        entries => entries.forEach(e => {
            if (e.isIntersecting) e.target.classList.add("visible");
            else e.target.classList.remove("visible");
        }),
        { threshold: 0.05, root: null, rootMargin: "0px 0px -5% 0px" }
    );

    els.forEach(el => { markIfInView(el); obs.observe(el); });
}
