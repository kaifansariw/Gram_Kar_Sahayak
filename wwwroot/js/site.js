// Gram Kar Sahayak - Client-Side Interactive Scripts

document.addEventListener("DOMContentLoaded", function () {
    // 1. FAQ Accordion & Tabs
    const faqTabs = document.querySelectorAll(".faq-tab-btn");
    const faqItems = document.querySelectorAll(".faq-accordion-item");

    if (faqTabs.length > 0) {
        faqTabs.forEach(tab => {
            tab.addEventListener("click", function () {
                faqTabs.forEach(t => t.classList.remove("active"));
                this.classList.add("active");

                const targetCategory = this.getAttribute("data-category");
                faqItems.forEach(item => {
                    if (targetCategory === "all" || item.getAttribute("data-category") === targetCategory) {
                        item.style.display = "block";
                    } else {
                        item.style.display = "none";
                    }
                });
            });
        });
    }

    // Toggle FAQ item open/close
    document.querySelectorAll(".faq-header").forEach(header => {
        header.addEventListener("click", function () {
            const body = this.nextElementSibling;
            const isOpen = body.style.display === "block";

            // Close all first if single accordion feel
            document.querySelectorAll(".faq-body").forEach(b => b.style.display = "none");
            document.querySelectorAll(".faq-chevron").forEach(c => c.textContent = "▼");

            if (!isOpen) {
                body.style.display = "block";
                const chevron = this.querySelector(".faq-chevron");
                if (chevron) chevron.textContent = "▲";
            }
        });
    });

    // 2. Dynamic Payment Allocation Preview (on amount input change)
    const amountInput = document.getElementById("PaymentAmountInput");
    const houseIdInput = document.getElementById("SelectedHouseId");
    const previewContainer = document.getElementById("LiveAllocationPreviewContainer");

    if (amountInput && houseIdInput && previewContainer) {
        let debounceTimer;
        amountInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const houseId = houseIdInput.value;
                const amount = parseFloat(amountInput.value) || 0;

                if (houseId && amount > 0) {
                    fetch(`/Payments/GetAllocationPreview?houseId=${houseId}&amount=${amount}`)
                        .then(res => res.json())
                        .then(data => {
                            if (data.success && data.preview) {
                                renderAllocationPreview(data.preview);
                            }
                        })
                        .catch(err => console.error("Allocation preview error", err));
                }
            }, 300);
        });
    }

    function getLocalizedTaxName(name, lang) {
        if (!name) return "";
        if (name.includes("ઘર") || name.includes("House") || name.includes("મકાન")) {
            return lang === "hi" ? "मकान कर" : lang === "en" ? "House Tax" : "ઘરવેરો";
        }
        if (name.includes("પાણી") || name.includes("Water")) {
            return lang === "hi" ? "पानी कर" : lang === "en" ? "Water Tax" : "પાણીવેરો";
        }
        if (name.includes("દિવાબત્તી") || name.includes("Light")) {
            return lang === "hi" ? "प्रकाश कर" : lang === "en" ? "Lighting Tax" : "દિવાબત્તી કર";
        }
        if (name.includes("સફાઈ") || name.includes("Sanitation")) {
            return lang === "hi" ? "सफाई कर" : lang === "en" ? "Sanitation Tax" : "સફાઈ કર";
        }
        if (name.includes("દુકાન") || name.includes("Shop")) {
            return lang === "hi" ? "दुकान कर" : lang === "en" ? "Commercial Tax" : "દુકાન વેરો";
        }
        return name;
    }

    function renderAllocationPreview(preview) {
        if (!previewContainer) return;
        const T = window.GramKarTranslations || {};
        const lang = window.GramKarLang || "gu";

        let html = `
            <div class="card-brutal card-brutal-cream" style="margin-top: 18px; padding: 18px; border-width: 2px;">
                <h4 style="margin-bottom: 12px; color: var(--highlight);">
                    ${T.allocationPreview || 'Payment Allocation Preview'}
                </h4>
                <div class="table-container-brutal" style="box-shadow: none; margin-bottom: 12px;">
                    <table class="table-brutal" style="font-size: 15px;">
                        <thead>
                            <tr>
                                <th>${T.taxName || 'Tax'}</th>
                                <th>${T.finYear || 'Year'}</th>
                                <th>${T.pendingLateFee || 'Pending Late Fee'}</th>
                                <th>${T.payingLateFee || 'Paying Late Fee'}</th>
                                <th>${T.pendingPrincipal || 'Pending Principal'}</th>
                                <th>${T.payingPrincipal || 'Paying Principal'}</th>
                                <th>${T.totalAllocated || 'Total Allocated'}</th>
                            </tr>
                        </thead>
                        <tbody>`;

        if (preview.items && preview.items.length > 0) {
            preview.items.forEach(item => {
                const localizedTax = getLocalizedTaxName(item.taxName, lang);
                html += `
                    <tr>
                        <td><strong>${localizedTax}</strong></td>
                        <td>${item.financialYear}</td>
                        <td>₹${item.pendingLateFeeBefore.toFixed(2)}</td>
                        <td style="color: var(--danger); font-weight: bold;">₹${item.lateFeeToPay.toFixed(2)}</td>
                        <td>₹${item.pendingPrincipalBefore.toFixed(2)}</td>
                        <td style="color: var(--leaf); font-weight: bold;">₹${item.principalToPay.toFixed(2)}</td>
                        <td><strong>₹${item.totalAllocated.toFixed(2)}</strong></td>
                    </tr>`;
            });
        } else {
            html += `<tr><td colspan="7" style="text-align:center;">${T.noPendingDues || 'No pending dues'}</td></tr>`;
        }

        html += `
                        </tbody>
                    </table>
                </div>
                <div style="display: flex; justify-content: space-between; font-weight: 800; font-size: 17px;">
                    <span>${T.totalPayment || 'Total Payment'}: <span style="color: var(--leaf);">₹${preview.totalAllocated.toFixed(2)}</span></span>
                    <span>${T.remainingBalanceAfter || 'Remaining Balance'}: <span style="color: var(--danger);">₹${preview.remainingBalanceAfter.toFixed(2)}</span></span>
                </div>
            </div>`;

        previewContainer.innerHTML = html;
    }

    // 3. Public Dues Lookup AJAX submit
    const lookupForm = document.getElementById("PublicDuesForm");
    const lookupResultDiv = document.getElementById("DuesLookupResult");

    if (lookupForm && lookupResultDiv) {
        lookupForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const formData = new FormData(lookupForm);
            const T = window.GramKarTranslations || {};

            lookupResultDiv.innerHTML = `<div style="text-align:center; padding: 20px; font-weight:bold;">${T.checkingDues || 'Checking dues...'}</div>`;

            fetch(lookupForm.action, {
                method: "POST",
                body: formData
            })
                .then(res => res.text())
                .then(html => {
                    lookupResultDiv.innerHTML = html;
                    lookupResultDiv.scrollIntoView({ behavior: 'smooth' });
                })
                .catch(err => {
                    lookupResultDiv.innerHTML = `<div class="toast-brutal toast-error">${T.errorTryAgain || 'Something went wrong. Please try again.'}</div>`;
                });
        });
    }
});

// Global Online UPI Payment Helpers (accessible from AJAX partial views)
window.toggleOnlinePaymentBox = function () {
    const box = document.getElementById('onlinePaymentBox');
    if (!box) return;
    if (box.style.display === 'none' || box.style.display === '') {
        box.style.display = 'block';
        box.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    } else {
        box.style.display = 'none';
    }
};

window.copyUpiId = function (text, btn) {
    if (!navigator.clipboard) return;
    navigator.clipboard.writeText(text).then(function () {
        const orig = btn.innerText;
        btn.innerText = '✓';
        setTimeout(() => { btn.innerText = orig; }, 1800);
    });
};

window.submitOnlinePayment = async function (e) {
    e.preventDefault();
    const form = document.getElementById('onlinePayForm');
    const btn = document.getElementById('btnConfirmPay');
    const alertBox = document.getElementById('onlinePayAlert');
    if (!form || !btn || !alertBox) return;

    const origText = btn.innerText;
    const T = window.GramKarTranslations || {};

    btn.disabled = true;
    btn.innerText = (T.processingPayment || 'Processing Payment...');
    alertBox.style.display = 'none';

    try {
        const formData = new FormData(form);
        const response = await fetch('/Home/ProcessOnlinePayment', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();
        if (result.success) {
            alertBox.style.display = 'block';
            alertBox.style.backgroundColor = '#EAF8EC';
            alertBox.style.color = '#1D7A37';
            alertBox.style.border = '2px solid #1D7A37';
            alertBox.innerText = (T.confirmAndGetReceipt || 'Payment Confirmed!') + ' - ' + (T.receiptNo || 'Receipt No.') + ': ' + result.receiptNo;

            setTimeout(() => {
                window.location.href = result.redirectUrl;
            }, 1000);
        } else {
            btn.disabled = false;
            btn.innerText = origText;
            alertBox.style.display = 'block';
            alertBox.style.backgroundColor = '#FFF2F2';
            alertBox.style.color = '#C92A2A';
            alertBox.style.border = '2px solid #C92A2A';
            alertBox.innerText = (result.message || T.errorTryAgain || 'Something went wrong. Please try again.');
        }
    } catch (err) {
        btn.disabled = false;
        btn.innerText = origText;
        alertBox.style.display = 'block';
        alertBox.style.backgroundColor = '#FFF2F2';
        alertBox.style.color = '#C92A2A';
        alertBox.style.border = '2px solid #C92A2A';
        alertBox.innerText = (T.errorTryAgain || 'Something went wrong. Please try again.');
    }
};
