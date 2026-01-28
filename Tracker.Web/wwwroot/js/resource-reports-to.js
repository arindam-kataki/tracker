// Add this JavaScript to Edit.cshtml @section Scripts or to a separate JS file

// ============================================
// Reports To Dropdown Management
// ============================================

// Load potential managers for a Reports To dropdown
function loadPotentialManagers(selectElement) {
    const serviceAreaId = selectElement.dataset.serviceAreaId;
    const resourceId = selectElement.dataset.resourceId || document.getElementById('resourceId')?.value;
    const currentValue = selectElement.value;
    
    if (!serviceAreaId) return;
    
    fetch(`/resources/potential-managers?serviceAreaId=${encodeURIComponent(serviceAreaId)}&excludeResourceId=${encodeURIComponent(resourceId || '')}`)
        .then(response => response.json())
        .then(managers => {
            // Clear existing options except the first "No Manager" option
            selectElement.innerHTML = '<option value="">-- No Manager --</option>';
            
            managers.forEach(manager => {
                if (manager.value) { // Skip empty value (already added above)
                    const option = document.createElement('option');
                    option.value = manager.value;
                    option.textContent = manager.text;
                    if (manager.value === currentValue) {
                        option.selected = true;
                    }
                    selectElement.appendChild(option);
                }
            });
        })
        .catch(error => {
            console.error('Error loading potential managers:', error);
        });
}

// Validate reports-to selection for circular references
function validateReportsTo(selectElement) {
    const serviceAreaId = selectElement.dataset.serviceAreaId;
    const resourceId = selectElement.dataset.resourceId || document.getElementById('resourceId')?.value;
    const reportsToId = selectElement.value;
    
    if (!resourceId || !serviceAreaId || !reportsToId) {
        // No validation needed for empty selection or new resources
        clearValidationError(selectElement);
        return;
    }
    
    fetch(`/resources/validate-reports-to?resourceId=${encodeURIComponent(resourceId)}&serviceAreaId=${encodeURIComponent(serviceAreaId)}&reportsToResourceId=${encodeURIComponent(reportsToId)}`)
        .then(response => response.json())
        .then(result => {
            if (!result.valid) {
                showValidationError(selectElement, result.message);
                selectElement.value = ''; // Reset to no manager
            } else {
                clearValidationError(selectElement);
            }
        })
        .catch(error => {
            console.error('Error validating reports-to:', error);
        });
}

function showValidationError(element, message) {
    clearValidationError(element);
    element.classList.add('is-invalid');
    const feedback = document.createElement('div');
    feedback.className = 'invalid-feedback';
    feedback.textContent = message;
    element.parentNode.appendChild(feedback);
}

function clearValidationError(element) {
    element.classList.remove('is-invalid');
    const existingFeedback = element.parentNode.querySelector('.invalid-feedback');
    if (existingFeedback) {
        existingFeedback.remove();
    }
}

// Initialize reports-to dropdowns on page load
document.addEventListener('DOMContentLoaded', function() {
    // Load managers for existing memberships
    document.querySelectorAll('.reports-to-select').forEach(select => {
        loadPotentialManagers(select);
        
        // Add change handler for validation
        select.addEventListener('change', function() {
            validateReportsTo(this);
            // Update hidden name field
            const nameInput = this.parentNode.querySelector('input[name$=".ReportsToName"]');
            if (nameInput) {
                nameInput.value = this.options[this.selectedIndex]?.text || '';
            }
        });
        
        // Load managers when dropdown is focused (in case new resources were added)
        select.addEventListener('focus', function() {
            loadPotentialManagers(this);
        });
    });
});

// When a new service area membership is added, initialize its reports-to dropdown
// Add this to the confirmAddServiceArea click handler, after the HTML is added to the container:
/*
    // Initialize reports-to dropdown for the new membership
    const newCard = container.lastElementChild;
    const reportsToSelect = newCard.querySelector('.reports-to-select');
    if (reportsToSelect) {
        loadPotentialManagers(reportsToSelect);
        
        reportsToSelect.addEventListener('change', function() {
            validateReportsTo(this);
            const nameInput = this.parentNode.querySelector('input[name$=".ReportsToName"]');
            if (nameInput) {
                nameInput.value = this.options[this.selectedIndex]?.text || '';
            }
        });
        
        reportsToSelect.addEventListener('focus', function() {
            loadPotentialManagers(this);
        });
    }
*/
