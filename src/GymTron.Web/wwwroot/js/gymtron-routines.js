/**
 * GymTron Routines Manager
 * Shared client-side controller for Routines Create and Edit views.
 */

(function (window) {
    'use strict';

    let config = {
        exerciseTypes: {},
        labels: {
            addTitlePrefix: 'Añadir ejercicio al',
            editTitle: 'Editar ejercicio',
            selectExerciseAlert: 'Por favor, selecciona un ejercicio.',
            cancelButtonText: 'Cancelar',
            backButtonText: 'Volver',
            days: ['', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo']
        }
    };

    let exerciseModal = null;
    let isAddingNew = false;
    let formChanged = false;
    let initialFormState = '';

    const EXERCISE_TYPE_WEIGHT = 1;
    const EXERCISE_TYPE_DURATION = 2;

    function initRoutineManager(userConfig) {
        if (userConfig) {
            if (userConfig.exerciseTypes) config.exerciseTypes = userConfig.exerciseTypes;
            if (userConfig.labels) config.labels = Object.assign({}, config.labels, userConfig.labels);
        }

        const modalEl = document.getElementById('exerciseModal');
        if (modalEl && window.bootstrap) {
            exerciseModal = new bootstrap.Modal(modalEl);
        }

        setTimeout(() => {
            initialFormState = captureFormState();
            updateCancelButton();
        }, 500);

        const form = document.getElementById('routineForm') || document.querySelector('form');
        if (form) {
            form.addEventListener('input', checkFormChanges);
            form.addEventListener('change', checkFormChanges);
        }

        const modalExercise = document.getElementById('modalExercise');
        if (modalExercise) {
            modalExercise.addEventListener('change', function () {
                toggleModalFieldsByType(parseInt(this.value, 10));

                if (isAddingNew) {
                    const selectedOption = this.options[this.selectedIndex];
                    const nameInput = document.getElementById('modalExerciseName');
                    if (nameInput) {
                        nameInput.value = selectedOption ? selectedOption.text : '';
                    }
                }
            });
        }
    }

    function toggleModalFieldsByType(exerciseId) {
        const exerciseType = config.exerciseTypes[exerciseId] || EXERCISE_TYPE_WEIGHT;
        const repetitionsGroup = document.querySelectorAll('.repetitions-group');
        const durationGroup = document.querySelectorAll('.duration-group');
        const repsMin = document.getElementById('modalRepsMin');
        const repsMax = document.getElementById('modalRepsMax');
        const duration = document.getElementById('modalDuration');

        if (exerciseType === EXERCISE_TYPE_DURATION) {
            repetitionsGroup.forEach(el => el.style.display = 'none');
            durationGroup.forEach(el => el.style.display = 'block');
            if (repsMin) repsMin.disabled = true;
            if (repsMax) repsMax.disabled = true;
            if (duration) duration.disabled = false;
        } else {
            repetitionsGroup.forEach(el => el.style.display = 'block');
            durationGroup.forEach(el => el.style.display = 'none');
            if (repsMin) repsMin.disabled = false;
            if (repsMax) repsMax.disabled = false;
            if (duration) duration.disabled = true;
        }
    }

    function openAddExerciseModal(dayOfWeek) {
        isAddingNew = true;

        const editingIndex = document.getElementById('editingIndex');
        if (editingIndex) editingIndex.value = '-1';

        const titleEl = document.getElementById('exerciseModalLabel');
        if (titleEl) {
            const dayName = getDayName(dayOfWeek);
            titleEl.textContent = `${config.labels.addTitlePrefix} ${dayName}`;
        }

        const exercisesInDay = Array.from(document.querySelectorAll(`input[name*="DayOfWeek"][value="${dayOfWeek}"]`)).length;
        const nextOrder = exercisesInDay + 1;

        const setVal = (id, val) => {
            const el = document.getElementById(id);
            if (el) el.value = val;
        };

        setVal('modalExercise', '');
        setVal('modalExerciseName', '');
        setVal('modalSeries', 3);
        setVal('modalRepsMin', 8);
        setVal('modalRepsMax', 12);
        setVal('modalDuration', 0);
        setVal('modalRestMin', 60);
        setVal('modalRestMax', 90);
        setVal('modalOrder', nextOrder);
        setVal('modalDayOfWeek', dayOfWeek);

        const altCheck = document.getElementById('modalAlternating');
        if (altCheck) altCheck.checked = false;

        toggleModalFieldsByType(0);

        if (exerciseModal) {
            exerciseModal.show();
        }
    }

    function editExercise(index) {
        isAddingNew = false;

        const editingIndex = document.getElementById('editingIndex');
        if (editingIndex) editingIndex.value = index;

        const titleEl = document.getElementById('exerciseModalLabel');
        if (titleEl) titleEl.textContent = config.labels.editTitle;

        const getInputValue = (prop) => {
            const el = document.querySelector(`input[name="Routine.Items[${index}].${prop}"]`);
            return el ? el.value : '';
        };

        const exerciseParamId = getInputValue('ExerciseParametersId');
        const dayOfWeek = getInputValue('DayOfWeek');
        const series = getInputValue('Series');
        const repsMin = getInputValue('RepetitionsMin');
        const repsMax = getInputValue('RepetitionsMax');
        const duration = getInputValue('Duration');
        const restMin = getInputValue('MinRestTimeInSeconds');
        const restMax = getInputValue('MaxRestTimeInSeconds');
        const alternatingValue = getInputValue('AlternatingSeries');
        const alternating = alternatingValue === 'true' || alternatingValue === 'True';
        const order = getInputValue('Position');

        const setVal = (id, val) => {
            const el = document.getElementById(id);
            if (el) el.value = val;
        };

        setVal('modalExercise', exerciseParamId);
        setVal('modalDayOfWeek', dayOfWeek);
        setVal('modalSeries', series);
        setVal('modalRepsMin', repsMin);
        setVal('modalRepsMax', repsMax);
        setVal('modalDuration', duration);
        setVal('modalRestMin', restMin);
        setVal('modalRestMax', restMax);
        setVal('modalOrder', order);

        const altCheck = document.getElementById('modalAlternating');
        if (altCheck) altCheck.checked = alternating;

        toggleModalFieldsByType(parseInt(exerciseParamId, 10));

        if (exerciseModal) {
            exerciseModal.show();
        }
    }

    function saveExerciseChanges() {
        const index = document.getElementById('editingIndex').value;
        const exerciseSelect = document.getElementById('modalExercise');
        const exerciseParamId = exerciseSelect ? exerciseSelect.value : '';

        if (!exerciseParamId) {
            alert(config.labels.selectExerciseAlert);
            return;
        }

        const getVal = (id) => {
            const el = document.getElementById(id);
            return el ? el.value : '';
        };

        const series = getVal('modalSeries');
        const repsMin = getVal('modalRepsMin');
        const repsMax = getVal('modalRepsMax');
        const duration = getVal('modalDuration');
        const restMin = getVal('modalRestMin');
        const restMax = getVal('modalRestMax');
        const altCheck = document.getElementById('modalAlternating');
        const alternating = altCheck ? altCheck.checked : false;
        const order = getVal('modalOrder');
        const dayOfWeek = getVal('modalDayOfWeek');

        const form = document.getElementById('routineForm') || document.querySelector('form');

        if (isAddingNew || index === '-1') {
            const allItems = document.querySelectorAll('input[name^="Routine.Items["]');
            const newIndex = Math.round(allItems.length / 12);
            const exerciseType = config.exerciseTypes[parseInt(exerciseParamId, 10)] || 1;

            const addField = (name, value) => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = `Routine.Items[${newIndex}].${name}`;
                input.value = value;
                form.appendChild(input);
            };

            addField('Id', '0');
            addField('DayOfWeek', dayOfWeek);
            addField('ExerciseParametersId', exerciseParamId);
            addField('ExerciseName', exerciseSelect.options[exerciseSelect.selectedIndex].text);
            addField('Series', series);
            addField('RepetitionsMin', repsMin);
            addField('RepetitionsMax', repsMax);
            addField('Duration', duration || '0');
            addField('MinRestTimeInSeconds', restMin);
            addField('MaxRestTimeInSeconds', restMax);
            addField('AlternatingSeries', alternating);
            addField('Position', order);
            addField('Type', exerciseType);

            formChanged = true;
            updateCancelButton();

            if (exerciseModal) exerciseModal.hide();
            form.submit();
        } else {
            const idx = parseInt(index, 10);
            const setFormValue = (prop, val) => {
                const el = document.querySelector(`input[name="Routine.Items[${idx}].${prop}"]`);
                if (el) el.value = val;
            };

            setFormValue('ExerciseParametersId', exerciseParamId);
            setFormValue('Series', series);
            setFormValue('RepetitionsMin', repsMin);
            setFormValue('RepetitionsMax', repsMax);
            setFormValue('Duration', duration);
            setFormValue('MinRestTimeInSeconds', restMin);
            setFormValue('MaxRestTimeInSeconds', restMax);
            setFormValue('AlternatingSeries', alternating);
            setFormValue('Position', order);

            formChanged = true;
            updateCancelButton();

            if (exerciseModal) exerciseModal.hide();
            form.submit();
        }
    }

    function getDayName(day) {
        return config.labels.days[day] || 'Desconocido';
    }

    function captureFormState() {
        const form = document.getElementById('routineForm') || document.querySelector('form');
        if (!form) return '';
        const inputs = form.querySelectorAll('input[type="hidden"]');
        const state = [];
        inputs.forEach(input => {
            state.push(`${input.name}=${input.value}`);
        });
        return state.sort().join('&');
    }

    function checkFormChanges() {
        const currentState = captureFormState();
        formChanged = currentState !== initialFormState;
        updateCancelButton();
    }

    function updateCancelButton() {
        const cancelButton = document.getElementById('cancelButton');
        if (cancelButton && formChanged) {
            cancelButton.textContent = config.labels.cancelButtonText;
        }
    }

    // Expose to window
    window.GymTronRoutines = {
        init: initRoutineManager,
        openAddExerciseModal: openAddExerciseModal,
        editExercise: editExercise,
        saveExerciseChanges: saveExerciseChanges,
        toggleModalFieldsByType: toggleModalFieldsByType,
        getDayName: getDayName
    };

})(window);
