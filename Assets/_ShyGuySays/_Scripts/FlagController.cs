using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlagController : MonoBehaviour {

    [SerializeField] private KMSelectable _colourblindButton;
    [SerializeField] private MainFlag[] _flags;

    private ShyGuySays _module;
    private Coroutine _executeQueue;
    private Coroutine _loopQueue;

    private Queue<FlagAction> _queue = new Queue<FlagAction>();

    private void Start() {
        _module = GetComponentInParent<ShyGuySays>();

        if (_flags.Length != 2) {
            throw new RankException("Is you man a bit stupid?");
        }

        _colourblindButton.OnInteract += delegate () {
            _flags[0].ColourblindModeActive = !_flags[0].ColourblindModeActive;
            _flags[1].ColourblindModeActive = !_flags[1].ColourblindModeActive;
            return false;
        };
    }

    private void Update() {
        if (_queue.Count > 0 && _executeQueue == null) {
            _executeQueue = StartCoroutine(ExecuteQueue());
        }
    }

    public void Enqueue(FlagAction action, bool clearExistingActions = false) {
        if (clearExistingActions) {
            StopQueue();
        }

        _queue.Enqueue(action);
    }

    public void ToggleColourblindMode() {

    }

    public void StopQueue() {
        _queue.Clear();
        if (_loopQueue != null) {
            StopCoroutine(_loopQueue);
        }
        _flags[0].Unflip();
        _flags[1].Unflip();
    }

    private IEnumerator ExecuteQueue() {
        while (_queue.Count > 0) {
            FlagAction nextAction = _queue.Dequeue();

            foreach (FlagRaise raise in nextAction.Raises) {
                _flags[raise.Position].Flip(raise.Colour, raise.Letter, nextAction.Speed);
            }

            yield return new WaitForSeconds(1 / (2 * nextAction.Speed));

            foreach (FlagRaise raise in nextAction.Raises) {
                if (raise.IsFake) {
                    _flags[raise.Position].Unflip();
                }
            }

            yield return new WaitForSeconds(1 / (2 * nextAction.Speed));

            foreach (FlagRaise raise in nextAction.Raises) {
                _flags[raise.Position].Unflip();
            }

            while (!(_flags[0].Ready && _flags[1].Ready)) {
                yield return null;
            }
        }

        _executeQueue = null;
    }

    public void EnqueueLoop(FlagAction[] actions, float waitTime) {
        _loopQueue = StartCoroutine(LoopQueue(actions, waitTime));
    }

    private IEnumerator LoopQueue(FlagAction[] actions, float waitSeconds) {
        StopQueue();
        while (true) {
            foreach (FlagAction action in actions) {
                Enqueue(action);
            }

            while (_queue.Count() != 0) {
                yield return null;
            }

            yield return new WaitForSeconds(waitSeconds);
        }
    }
}
