
using UnityEngine;
using UnityEngine.InputSystem;

namespace KOOPUtil
{
    public static class InputActionExtensions
    {
        /// <summary>
        /// Finds the first group of <see cref="InputControl"/>s and puts them in <see cref="_validInputControlCache"/>.
        /// </summary>
        /// <param name="inputAction">The action to look for valid controls in.</param>
        /// <param name="bindingGroup">A binding group used to filter the binding results.</param>
        /// <returns>The number of input controls that were placed in <see cref="_validInputControlCache"/>.</returns>
        /// <remarks>
        /// If the first valid input control is connected to a binding that isn't a composite or part of a composite, it will qualify
        /// as the first group, if the first valid input control is part of a composite, it will include all other bindings with the same composite. 
        /// </remarks>
        public static int FindValidInputControlsNonAlloc(this InputAction inputAction, string bindingGroup, InputControl[] inputControls)
        {
            // create the binding mask
            InputBinding bindingMask = InputBinding.MaskByGroup(bindingGroup);

            // find each input control
            int firstCompositeIndex = -1;
            int countFound = 0;
            foreach (InputControl inputControl in inputAction.controls)
            {
                if (countFound == inputControls.Length)
                {
                    // Found all the controls we need!
                    return countFound;
                }

                // get the binding index of the control and ignore it if it's not part of the binding mask
                int bindingIndex = inputAction.GetBindingIndexForControl(inputControl);
                if (bindingIndex == -1 || !bindingMask.Matches(inputAction.bindings[bindingIndex]))
                    continue;

                InputBinding binding = inputAction.bindings[bindingIndex];

                // if the binding is part of a composite, make sure that we're only using the first composite
                if (binding.isPartOfComposite)
                {
                    // if the binding is part of a composite, find the composite's binding index
                    int compositeBindingIndex = -1;
                    for (int i = bindingIndex - 1; i >= 0; i--) // the composite's binding index will always be before the part's
                    {
                        if (inputAction.bindings[i].isComposite)
                        {
                            compositeBindingIndex = i;
                            break;
                        }
                    }

                    if (firstCompositeIndex == -1)
                    {
                        // if this is the first composite found, track it
                        firstCompositeIndex = compositeBindingIndex;
                    }
                    else if (firstCompositeIndex != compositeBindingIndex)
                    {
                        // if this isn't from the first composite, then 
                        break;
                    }

                    // add the input control to the cache
                    inputControls[countFound] = inputControl;
                    countFound++;
                }
                else if (!binding.isComposite) // this should always evaluate to true as input controls can't provide bindings
                {
                    // if this binding isn't a composite or part of one, then it's the first and only binding we need
                    inputControls[countFound] = inputControl;
                    countFound++;
                    break;
                }
            }

            return countFound;
        }

        public static bool TryGetBindingForControl(this InputControl inputControl, InputAction inputAction, string bindingGroup, out InputBinding inputBinding)
        {
            // initialize out variable
            inputBinding = default;

            // create the binding mask
            InputBinding bindingMask = InputBinding.MaskByGroup(bindingGroup);

            // get the binding index of the control and ignore it if it's not part of the binding mask
            int bindingIndex = inputAction.GetBindingIndexForControl(inputControl);

            if (bindingIndex == -1 || !bindingMask.Matches(inputAction.bindings[bindingIndex]))
            {
                return false;
            }

            inputBinding = inputAction.bindings[bindingIndex];
            return true;
        }
    }
}